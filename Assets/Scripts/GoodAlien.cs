using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GoodAlien : MonoBehaviour
{
    public int ID;
    public float moveSpeed = 0.02f;
    public float rotationSpeed = 20f;
    public int maxLives = 5; // Vidas máximas del alien bueno
    public float invulnerabilityTime = 2f; // Tiempo de invulnerabilidad después de ser golpeado
    
    private ARPlane movePlane;
    private float colliderHeight;
    private bool isMoving = false;
    private int currentLives; // Vidas actuales
    private bool isInvulnerable = false; // Estado de invulnerabilidad
    private float invulnerabilityTimer = 0f; // Timer para la invulnerabilidad
    
    // Evento para cuando el alien bueno es capturado
    public delegate void GoodAlienCapturedEventHandler();
    public event GoodAlienCapturedEventHandler OnGoodAlienCaptured;
    
    // Evento para cuando el alien bueno pierde una vida
    public delegate void GoodAlienHitEventHandler(int remainingLives);
    public event GoodAlienHitEventHandler OnGoodAlienHit;
    
    void Start()
    {
        // Configurar el alien bueno
        colliderHeight = transform.localScale.y * GetComponent<CapsuleCollider>().height;
        currentLives = maxLives;
    }
    
    public void StartMoving(ARPlane plane)
    {
        movePlane = plane;
        // El alien bueno se queda estático en el centro
        transform.position = plane.center + Vector3.up * colliderHeight / 2;
        isMoving = true;
        currentLives = maxLives; // Reiniciar vidas
        isInvulnerable = false; // Reiniciar estado de invulnerabilidad
    }
    
    void Update()
    {
        if (!isMoving) return;
        
        // El alien bueno se queda estático, no se mueve
        // Solo manejar la invulnerabilidad
        
        // Hacer que siempre mire hacia la cámara
        LookAtCamera();
        
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;
                // Restaurar color normal
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.white;
                }
            }
        }
    }
    
    // Método para hacer que el alien siempre mire hacia la cámara
    void LookAtCamera()
    {
        // Obtener la posición de la cámara principal
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Calcular la dirección hacia la cámara
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            
            // Mantener la rotación solo en el eje Y (no inclinar hacia arriba/abajo)
            directionToCamera.y = 0f;
            
            // Si hay dirección válida, rotar hacia la cámara
            if (directionToCamera.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    

    
    public void StopMoving()
    {
        isMoving = false;
    }
    
    // Método para cuando el alien bueno es golpeado por el jugador
    public void OnHitByPlayer()
    {
        // Verificar si está invulnerable
        if (isInvulnerable)
        {
            Debug.Log("¡Alien bueno está invulnerable! No recibe daño.");
            return;
        }
        
        currentLives--;
        Debug.Log($"¡Alien bueno golpeado! Vidas restantes: {currentLives}");
        
        // Activar invulnerabilidad
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityTime;
        
        // Notificar que fue golpeado
        OnGoodAlienHit?.Invoke(currentLives);
        
        if (currentLives <= 0)
        {
            Debug.Log("¡Alien bueno destruido por el jugador!");
            OnGoodAlienCaptured?.Invoke();
            Destroy(gameObject);
        }
        else
        {
            // Efecto visual de daño con invulnerabilidad
            StartCoroutine(DamageEffect());
        }
    }
    
    // Método para cuando el alien bueno es tocado por un alien malo
    public void OnCapturedByBadAlien()
    {
        // Verificar si está invulnerable
        if (isInvulnerable)
        {
            Debug.Log("¡Alien bueno está invulnerable! No puede ser dañado.");
            return;
        }
        
        // Perder una vida en lugar de ser capturado inmediatamente
        currentLives--;
        Debug.Log($"¡Alien bueno tocado por alien malo! Vidas restantes: {currentLives}");
        
        // Activar invulnerabilidad
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityTime;
        
        // Notificar que fue golpeado
        OnGoodAlienHit?.Invoke(currentLives);
        
        if (currentLives <= 0)
        {
            Debug.Log("¡Alien bueno destruido por alien malo!");
            OnGoodAlienCaptured?.Invoke();
            Destroy(gameObject);
        }
        else
        {
            // Efecto visual de daño con invulnerabilidad
            StartCoroutine(DamageEffect());
        }
    }
    
    // Método para cuando el alien bueno es capturado por el jugador (por error) - MANTENER COMPATIBILIDAD
    public void OnCapturedByPlayer()
    {
        OnHitByPlayer(); // Ahora usa el sistema de vidas
    }
    
    // Efecto visual cuando recibe daño
    IEnumerator DamageEffect()
    {
        // Hacer que el alien parpadee durante la invulnerabilidad
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            float effectDuration = invulnerabilityTime;
            float elapsed = 0f;
            
            while (elapsed < effectDuration && isInvulnerable)
            {
                renderer.material.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                renderer.material.color = Color.white;
                yield return new WaitForSeconds(0.2f);
                elapsed += 0.4f;
            }
            
            // Restaurar color original
            renderer.material.color = originalColor;
        }
    }
    
    // Getter para obtener las vidas actuales
    public int GetCurrentLives()
    {
        return currentLives;
    }
} 