using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // public Game Objects
    [Header("Aliens")]
    public GameObject goodAlienPrefab;
    public GameObject badAlienPrefab;
    public int ammo = 7;

    [Header("Level Settings")]
    public int currentLevel = 1;
    public int maxLevel = 3;
    
    // Configuración por nivel
    private int[] badAliensPerLevel = { 2, 3, 4 }; // Aliens malos por nivel
    private float[] badAlienSpeedPerLevel = { 0.005f, 0.007f, 0.009f }; // Velocidad aumentada por nivel
    private int maxAmmoPerLevel = 7; // Límite de esferas por nivel

    [Header("UI Canvas Objects")]
    public GameObject planeSearchingCanvas;
    public GameObject selectPlaneCanvas;
    public GameObject startButton;
    public GameObject gameUI;
    public Text scoreTxt;
    public Text levelTxt; // Nuevo: mostrar nivel actual
    public Text livesTxt; // Nuevo: mostrar vidas del alien bueno
    public GameObject ammoImagePrefab;
    public GameObject ammoImageGrid;
    public GameObject playAgainButton;
    public GameObject leaderBoardButton;
    public GameObject leaderBoardUI;
    public GameObject victoryUI; // Nuevo: pantalla de victoria
    public GameObject defeatUI; // Nuevo: pantalla de derrota
    
    [Header("Victory/Defeat UI Text")]
    public TextMeshProUGUI victoryScoreText; // Texto de puntuación en pantalla de victoria
    public TextMeshProUGUI defeatScoreText; // Texto de puntuación en pantalla de derrota
    
    [Header("Lives System")]
    public GameObject lifeImagePrefab; // Prefab del PNG de vida
    public GameObject livesImageGrid; // Grid para mostrar las vidas

    [Header("Sounds")]
    public AudioSource EndingSound;
    public AudioSource planeSelectedSound;
    public AudioSource backgroundMusic;
    public AudioSource victorySound; // Nuevo: sonido de victoria
    public AudioSource defeatSound; // Nuevo: sonido de derrota
    public AudioSource goodAlienHitSound; // Nuevo: sonido cuando el alien bueno es golpeado

    [Header("Scripts")]
    public Leaderboard leaderBoard;

    [Header("Materials")]
    public Material PlaneOcclusionMaterial;

    // private variables
    int totalPoints = 0;
    int badAliensRemaining = 0;
    bool goodAlienAlive = true;
    int goodAlienLives = 5; // Vidas del alien bueno

    // private GameObjects
    ARPlane selectedPlane = null;    
    ARRaycastManager raycastManager;
    ARPlaneManager planeManager;
    SlingShot slingShot;
    ARSession session;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();
    Dictionary<int, GameObject> badAliens = new Dictionary<int, GameObject>();
    GameObject goodAlien = null;

    //Events
    public delegate void PlaneSelectedEventHandler(ARPlane thePlane);
    public event PlaneSelectedEventHandler OnPlaneSelected;
    
    void Awake()
    {
        session = FindObjectOfType<ARSession>();
        session.Reset();
    }    
    
    // Start is called before the first frame update
    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        planeManager = FindObjectOfType<ARPlaneManager>();
        slingShot = FindObjectOfType<SlingShot>();
        
        planeManager.planesChanged += PlanesFound;
        OnPlaneSelected += PlaneSelected;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && selectedPlane == null && planeManager.trackables.count > 0)
        {
            SelectPlane();
        }
    }
    
    private void SelectPlane()
    {
        Touch touch = Input.GetTouch(0);
        

        if (touch.phase == TouchPhase.Began)
        {
            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                ARRaycastHit hit = hits[0];
                selectedPlane =  planeManager.GetPlane(hit.trackableId);                
                selectedPlane.GetComponent<LineRenderer>().positionCount = 0;

                selectedPlane.GetComponent<Renderer>().material = PlaneOcclusionMaterial;
                
                foreach(ARPlane plane in planeManager.trackables)
                {
                    if (plane != selectedPlane)
                    {
                        plane.gameObject.SetActive(false);
                    }
                }
                planeManager.enabled = false;
                
                // Desactivar el canvas de selección de plano ANTES de invocar el evento
                selectPlaneCanvas.SetActive(false);
                
                // Pequeña pausa para evitar el flash del menú inicial
                StartCoroutine(DelayedPlaneSelected());
            }
        }
    }
    
    // Corrutina para evitar el flash del menú inicial
    IEnumerator DelayedPlaneSelected()
    {
        yield return new WaitForEndOfFrame();
        OnPlaneSelected?.Invoke(selectedPlane);
    }
    


    void SetMaterialTransparent(ARPlane plane)
    {        
        foreach (Material material in plane.GetComponent<Renderer>().materials)
        {
            material.SetFloat("_Mode", 2);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.renderQueue = 3000;
        }
    }
    
    void PlanesFound(ARPlanesChangedEventArgs args)
    {
        if (selectedPlane == null && planeManager.trackables.count > 0)
        {
            planeSearchingCanvas.SetActive(false);
            selectPlaneCanvas.SetActive(true);
            planeManager.planesChanged -= PlanesFound;
        }
    }

    void PlaneSelected(ARPlane plane)
    {
        planeSelectedSound.Play();
        ClearAllAliens();
        
        // Habilitar el SlingShot para el nuevo plano
        if (slingShot != null)
        {
            slingShot.enabled = true;
        }
        
        // Guardar el plano seleccionado para usarlo después
        selectedPlane = plane;
        
        // Configurar el plano
        selectedPlane.GetComponent<LineRenderer>().positionCount = 0;
        selectedPlane.GetComponent<Renderer>().material = PlaneOcclusionMaterial;
        
        // NO instanciar aliens aquí - solo mostrar el botón Start
        startButton.SetActive(true);
        
        // Asegurar que el selectPlaneCanvas esté desactivado
        if (selectPlaneCanvas != null)
        {
            selectPlaneCanvas.SetActive(false);
        }
    }

    void ClearAllAliens()
    {
        // Limpiar alien bueno
        if (goodAlien != null)
        {
            Destroy(goodAlien);
            goodAlien = null;
        }
        
        // Limpiar aliens malos
        foreach (KeyValuePair<int, GameObject> badAlien in badAliens)
        {
            Destroy(badAlien.Value);
        }
        badAliens.Clear();
    }

    void OnGoodAlienCaptured()
    {
        Debug.Log("=== ALIEN BUENO CAPTURADO ===");
        goodAlienAlive = false;
        Debug.Log("¡El alien bueno ha sido capturado! ¡Has perdido!");
        ShowDefeatScreen();
    }
    
    // Nuevo método para manejar cuando el alien bueno es golpeado
    void OnGoodAlienHit(int remainingLives)
    {
        Debug.Log($"=== ALIEN BUENO GOLPEADO - Vidas restantes: {remainingLives} ===");
        goodAlienLives = remainingLives;
        
        // Reproducir sonido de daño
        if (goodAlienHitSound != null)
        {
            goodAlienHitSound.Play();
        }
        
        // El texto de vidas ya no es necesario, solo las vidas visuales
        
        // Actualizar vidas visuales
        UpdateLivesVisual();
        
        Debug.Log($"¡Alien bueno golpeado! Vidas restantes: {goodAlienLives}");
        
        // Si se quedó sin vidas, mostrar derrota
        if (goodAlienLives <= 0)
        {
            Debug.Log("=== SIN VIDAS - MOSTRANDO DERROTA ===");
            Debug.Log($"=== PUNTUACIÓN FINAL ANTES DE ShowDefeatScreen: {totalPoints} ===");
            ShowDefeatScreen();
        }
    }

    void UpdateGameWhenHitBadAlien(int id, int points)
    {
        badAliens.Remove(id);
        badAliensRemaining--;
        totalPoints += points;
        Debug.Log($"=== PUNTOS ACUMULADOS: {totalPoints} (añadidos: {points}) ===");
        scoreTxt.text = totalPoints.ToString();
        
        // NO recargar esferas al capturar aliens malos
        // Las esferas solo se recargan automáticamente cuando golpean algo
        
        if (badAliensRemaining == 0 && goodAlienAlive)
        {
            // Victoria en este nivel
            if (currentLevel < maxLevel)
            {
                // Pasar al siguiente nivel
                currentLevel++;
                StartCoroutine(NextLevel());
            }
            else
            {
                // Victoria final
                ShowVictoryScreen();
            }
        }
    }

    IEnumerator NextLevel()
    {
        // Pausa antes del siguiente nivel
        yield return new WaitForSeconds(2f);
        
        // Limpiar y reiniciar para el nuevo nivel
        ClearAllAliens();
        
        // Reiniciar munición al límite máximo
        if (slingShot != null)
        {
            slingShot.AmmoLeft = maxAmmoPerLevel;
        }
        
        PlaneSelected(selectedPlane);
        levelTxt.text = "Nivel " + currentLevel;
    }

    void ShowVictoryScreen()
    {
        Debug.Log("=== MOSTRANDO PANTALLA DE VICTORIA ===");
        Debug.Log($"=== PUNTUACIÓN AL INICIO DE ShowVictoryScreen: {totalPoints} ===");
        
        // Detener el juego
        goodAlienAlive = false;
        
        // Desactivar el SlingShot para que no se puedan lanzar más proyectiles
        if (slingShot != null)
        {
            slingShot.enabled = false;
            slingShot.Clear(); // Limpiar munición actual
        }
        
        // Detener el movimiento de todos los aliens
        if (goodAlien != null)
        {
            GoodAlien goodAlienScript = goodAlien.GetComponent<GoodAlien>();
            if (goodAlienScript != null)
            {
                goodAlienScript.StopMoving();
            }
        }
        
        foreach (KeyValuePair<int, GameObject> badAlienPair in badAliens)
        {
            BadAlien badAlienScript = badAlienPair.Value.GetComponent<BadAlien>();
            if (badAlienScript != null)
            {
                badAlienScript.StopMoving();
            }
        }
        
        // Detener música y reproducir sonido de victoria
        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }
        
        if (victorySound != null)
        {
            victorySound.Play();
        }
        
        // Cambiar UI con fondo verde
        gameUI.SetActive(false);
        victoryUI.SetActive(true);
        
        // Efecto de fade-in para la pantalla de victoria
        StartCoroutine(FadeInVictoryScreen());
        
        // Cambiar el color de fondo a verde
        Canvas canvas = victoryUI.GetComponent<Canvas>();
        if (canvas != null)
        {
            CanvasGroup canvasGroup = victoryUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = victoryUI.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1f;
        }
        
        // Actualizar texto de puntuación en pantalla de victoria
        Debug.Log($"=== PUNTUACIÓN EN VICTORIA: {totalPoints} ===");
        if (victoryScoreText != null)
        {
            victoryScoreText.text = "Puntuación: " + totalPoints.ToString();
        }
        
        // Asegurar que los botones de la pantalla de victoria estén activos
        Transform victoryButtonsTransform = victoryUI.transform.Find("Buttons");
        if (victoryButtonsTransform != null)
        {
            Transform restartButton = victoryButtonsTransform.Find("Restart");
            Transform quitButton = victoryButtonsTransform.Find("Quit");
            
            if (restartButton != null)
                restartButton.gameObject.SetActive(true);
            if (quitButton != null)
                quitButton.gameObject.SetActive(true);
        }
        
        leaderBoard.SetLeader(totalPoints);
        
        Debug.Log("=== JUEGO DETENIDO - PANTALLA DE VICTORIA ACTIVA ===");
    }

    void ShowDefeatScreen()
    {
        Debug.Log("=== MOSTRANDO PANTALLA DE DERROTA ===");
        Debug.Log($"=== PUNTUACIÓN AL INICIO DE ShowDefeatScreen: {totalPoints} ===");
        
        // Detener el juego
        goodAlienAlive = false;
        
        // Desactivar el SlingShot para que no se puedan lanzar más proyectiles
        if (slingShot != null)
        {
            slingShot.enabled = false;
            slingShot.Clear(); // Limpiar munición actual
        }
        
        // Detener el movimiento de todos los aliens
        if (goodAlien != null)
        {
            GoodAlien goodAlienScript = goodAlien.GetComponent<GoodAlien>();
            if (goodAlienScript != null)
            {
                goodAlienScript.StopMoving();
            }
        }
        
        foreach (KeyValuePair<int, GameObject> badAlienPair in badAliens)
        {
            BadAlien badAlienScript = badAlienPair.Value.GetComponent<BadAlien>();
            if (badAlienScript != null)
            {
                badAlienScript.StopMoving();
            }
        }
        
        // Detener música y reproducir sonido de derrota
        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }
        
        if (defeatSound != null)
        {
            defeatSound.Play();
        }
        
        // Cambiar UI con fondo rojo
        gameUI.SetActive(false);
        defeatUI.SetActive(true);
        
        // Efecto de fade-in para la pantalla de derrota
        StartCoroutine(FadeInDefeatScreen());
        
        // Cambiar el color de fondo a rojo
        Canvas canvas = defeatUI.GetComponent<Canvas>();
        if (canvas != null)
        {
            CanvasGroup canvasGroup = defeatUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = defeatUI.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1f;
        }
        
        // Actualizar texto de puntuación en pantalla de derrota
        Debug.Log($"=== PUNTUACIÓN EN DERROTA: {totalPoints} ===");
        if (defeatScoreText != null)
        {
            defeatScoreText.text = "Puntuación: " + totalPoints.ToString();
        }
        
        // Asegurar que los botones de la pantalla de derrota estén activos
        Transform defeatButtonsTransform = defeatUI.transform.Find("Buttons");
        if (defeatButtonsTransform != null)
        {
            Transform restartButton = defeatButtonsTransform.Find("Restart");
            Transform quitButton = defeatButtonsTransform.Find("Quit");
            
            if (restartButton != null)
                restartButton.gameObject.SetActive(true);
            if (quitButton != null)
                quitButton.gameObject.SetActive(true);
        }
        
        leaderBoard.SetLeader(totalPoints);
        
        Debug.Log("=== JUEGO DETENIDO - PANTALLA DE DERROTA ACTIVA ===");
    }
    
    // Método para actualizar las vidas visuales
    void UpdateLivesVisual()
    {
        if (livesImageGrid == null || lifeImagePrefab == null) return;
        
        // Limpiar vidas existentes
        foreach (Transform child in livesImageGrid.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Crear nuevas vidas según las vidas restantes
        for (int i = 0; i < goodAlienLives; i++)
        {
            GameObject lifeGO = Instantiate(lifeImagePrefab);
            lifeGO.transform.SetParent(livesImageGrid.transform, false);
        }
    }

    void SpawnAliens()
    {
        if (selectedPlane == null) return;
        
        // Instanciar el alien bueno en el centro
        Vector3 goodAlienPos = selectedPlane.center;
        goodAlien = Instantiate(goodAlienPrefab, goodAlienPos, selectedPlane.transform.rotation, selectedPlane.transform);
        goodAlien.transform.localScale = Vector3.one * 0.1f;
        
        GoodAlien goodAlienScript = goodAlien.GetComponent<GoodAlien>();
        goodAlienScript.OnGoodAlienCaptured += OnGoodAlienCaptured;
        goodAlienScript.OnGoodAlienHit += OnGoodAlienHit;
        
        // Instanciar aliens malos en las esquinas, lejos del centro
        int badAliensCount = badAliensPerLevel[currentLevel - 1];
        badAliensRemaining = badAliensCount;
        
        // Calcular posiciones en las esquinas del plano
        float planeWidth = selectedPlane.size.x;
        float planeHeight = selectedPlane.size.y;
        float margin = Mathf.Min(planeWidth, planeHeight) * 0.5f; // Aumentado de 0.3f a 0.5f
        
        for (int i = 1; i <= badAliensCount; i++)
        {
            Vector3 spawnPos;
            
            // Distribuir los aliens malos en las esquinas
            switch (i)
            {
                case 1: // Esquina superior izquierda
                    spawnPos = selectedPlane.center + new Vector3(-margin, 0, margin);
                    break;
                case 2: // Esquina superior derecha
                    spawnPos = selectedPlane.center + new Vector3(margin, 0, margin);
                    break;
                case 3: // Esquina inferior izquierda
                    spawnPos = selectedPlane.center + new Vector3(-margin, 0, -margin);
                    break;
                case 4: // Esquina inferior derecha
                    spawnPos = selectedPlane.center + new Vector3(margin, 0, -margin);
                    break;
                default: // Para más de 4 aliens, posiciones aleatorias
                    Vector3 randomOffset = new Vector3(
                        UnityEngine.Random.Range(-margin, margin),
                        0,
                        UnityEngine.Random.Range(-margin, margin)
                    );
                    spawnPos = selectedPlane.center + randomOffset;
                    break;
            }
            
            GameObject badAlien = Instantiate(badAlienPrefab, spawnPos, selectedPlane.transform.rotation, selectedPlane.transform);
            badAlien.transform.localScale = Vector3.one * 0.1f;
            
            BadAlien badAlienScript = badAlien.GetComponent<BadAlien>();
            badAlienScript.ID = i;
            badAlienScript.moveSpeed = badAlienSpeedPerLevel[currentLevel - 1];
            badAlienScript.OnBadAlienCaptured += UpdateGameWhenHitBadAlien;
            badAliens.Add(i, badAlien);
        }
    }

    public void StartGame()
    {
        Debug.Log("=== INICIANDO JUEGO ===");
        
        // Instanciar aliens
        SpawnAliens();
        
        Debug.Log($"Aliens creados - Bueno: {goodAlien != null}, Malos: {badAliens.Count}");
        
        // Iniciar movimiento de todos los aliens
        if (goodAlien != null)
        {
            GoodAlien goodAlienScript = goodAlien.GetComponent<GoodAlien>();
            goodAlienScript.StartMoving(selectedPlane);
            Debug.Log("Alien bueno iniciado");
        }
        
        foreach (KeyValuePair<int, GameObject> badAlienPair in badAliens)
        {
            BadAlien badAlienScript = badAlienPair.Value.GetComponent<BadAlien>();
            GoodAlien goodAlienScript = goodAlien.GetComponent<GoodAlien>();
            badAlienScript.StartMoving(selectedPlane, goodAlienScript);
            Debug.Log($"Alien malo {badAlienPair.Key} iniciado");
        }
        
        // Configurar el juego
        slingShot.enabled = true; // Asegurar que el SlingShot esté habilitado
        slingShot.AmmoLeft = maxAmmoPerLevel; // Usar el límite por nivel
        slingShot.OnReload += SlingShootReload;
        // NO llamar a Reload() aquí, se hará después de crear las imágenes
        totalPoints = 0;
        scoreTxt.text = totalPoints.ToString();
        levelTxt.text = "Nivel " + currentLevel;
        
        // Inicializar vidas del alien bueno
        goodAlienLives = 5;
        goodAlienAlive = true; // Asegurar que esté vivo
        
        // Inicializar vidas visuales
        UpdateLivesVisual();
        
        Debug.Log($"Juego iniciado - Vidas: {goodAlienLives}, Aliens malos: {badAliensRemaining}");
        
        startButton.SetActive(false);
        gameUI.SetActive(true);

        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
        }

        // Limpiar imágenes de ammo existentes
        foreach (Transform child in ammoImageGrid.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Crear imágenes de ammo según el límite
        for (int i = 0; i < maxAmmoPerLevel; i++)
        {
            GameObject ammoGO = Instantiate(ammoImagePrefab);
            ammoGO.transform.SetParent(ammoImageGrid.transform, false);
        }
        
        // Ahora sí recargar para crear la primera esfera
        slingShot.Reload();
    }
    
    void SlingShootReload(int ammoLeft)
    {
        // Solo destruir imágenes cuando realmente se pierde munición
        // El número de imágenes debe coincidir con AmmoLeft
        int currentImages = ammoImageGrid.transform.childCount;
        
        // Si hay más imágenes que munición, destruir las extras
        while (currentImages > ammoLeft && currentImages > 0)
        {
            Destroy(ammoImageGrid.transform.GetChild(0).gameObject);
            currentImages--;
        }
        
        // Verificar si se acabó la munición
        if (ammoLeft == 0)
        {
            if (goodAlienAlive && badAliensRemaining > 0)
            {
                // Sin munición pero aún hay aliens malos
                ShowDefeatScreen();
            }
            else
            {
                ShowPlayAgainButton();            
                ShowLeaderBoard();
            }
        }
    }
    
    public void ShowPlayAgainButton()
    {
        EndingSound.Play();
        
        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }
        
        leaderBoard.SetLeader(totalPoints);
        foreach (Transform ammoImge in ammoImageGrid.transform)
        {
            Destroy(ammoImge.gameObject);
        }
        slingShot.Clear();
        slingShot.OnReload -= SlingShootReload;
        
        // Mostrar botones de Restart y Quit en lugar de PlayAgain
        // playAgainButton.SetActive(true);
        // leaderBoardButton.SetActive(true);
        
        // Activar los botones de Restart y Quit que están en Game/Buttons/
        Transform buttonsTransform = gameUI.transform.Find("Buttons");
        if (buttonsTransform != null)
        {
            Transform restartButton = buttonsTransform.Find("Restart");
            Transform quitButton = buttonsTransform.Find("Quit");
            
            if (restartButton != null)
                restartButton.gameObject.SetActive(true);
            if (quitButton != null)
                quitButton.gameObject.SetActive(true);
        }
    }
    
    public void ShowLeaderBoard()
    {
        leaderBoard.PrintLeaderBoard();
        leaderBoardUI.SetActive(true);
    }

    public void PlayAgain()
    {
        Debug.Log("=== REINICIANDO JUEGO (PlayAgain) ===");
        
        // Ocultar UI de victoria/derrota
        victoryUI.SetActive(false);
        defeatUI.SetActive(false);
        
        // Habilitar el SlingShot
        if (slingShot != null)
        {
            slingShot.enabled = true;
        }
        
        // Limpiar UI del juego
        leaderBoardButton.SetActive(false);
        leaderBoardUI.SetActive(false);
        
        // Reiniciar variables del juego
        currentLevel = 1; // Reiniciar al nivel 1
        goodAlienAlive = true;
        goodAlienLives = 5; // Reiniciar vidas
        totalPoints = 0; // Reiniciar puntuación
        
        // Limpiar aliens y reiniciar
        ClearAllAliens();
        
        // Reiniciar el plano seleccionado sin llamar a PlaneSelected
        if (selectedPlane != null)
        {
            selectedPlane.GetComponent<LineRenderer>().positionCount = 0;
            selectedPlane.GetComponent<Renderer>().material = PlaneOcclusionMaterial;
        }
        
        // Mostrar el botón Start directamente
        startButton.SetActive(true);
        
        EndingSound.Stop();
    }
    
    // Nuevo método para el botón Restart
    public void RestartLevel()
    {
        Debug.Log("=== REINICIANDO NIVEL ACTUAL ===");
        
        // Ocultar botones de Restart y Quit
        Transform buttonsTransform = gameUI.transform.Find("Buttons");
        if (buttonsTransform != null)
        {
            Transform restartButton = buttonsTransform.Find("Restart");
            Transform quitButton = buttonsTransform.Find("Quit");
            
            if (restartButton != null)
                restartButton.gameObject.SetActive(false);
            if (quitButton != null)
                quitButton.gameObject.SetActive(false);
        }
        
        // Habilitar el SlingShot
        if (slingShot != null)
        {
            slingShot.enabled = true;
        }
        
        // Reiniciar el nivel actual
        goodAlienAlive = true;
        goodAlienLives = 5;
        ClearAllAliens();
        SpawnAliens();
        StartGame();
        EndingSound.Stop();
    }
    
    // Nuevo método para el botón Quit
    public void QuitToMenu()
    {
        // Volver al menú principal
        SceneManager.LoadScene("MainMenu");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene("ARSlingshotGame");
    }
    

    
    // Animación de fade-in para pantalla de victoria
    IEnumerator FadeInVictoryScreen()
    {
        CanvasGroup canvasGroup = victoryUI.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = victoryUI.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        
        float duration = 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    // Animación de fade-in para pantalla de derrota
    IEnumerator FadeInDefeatScreen()
    {
        CanvasGroup canvasGroup = defeatUI.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = defeatUI.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        
        float duration = 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
}
