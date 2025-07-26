using UnityEngine;
using UnityEngine.SceneManagement;

public class ContextPanelManager : MonoBehaviour
{
    public GameObject contextPanel;
    public GameObject mainMenuPanel; // El menú principal (botones Jugar, Salir, etc.)

    void Start()
    {
        // Al inicio, mostrar solo el menú principal
        if (contextPanel != null) contextPanel.SetActive(false);
    }

    public void AcceptTerms()
    {
        Debug.Log("=== MOSTRANDO CONTEXT PANEL ===");
        
        // OCULTAR el menú y MOSTRAR el contextPanel
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (contextPanel != null) contextPanel.SetActive(true);
    }

    public void DeclineTerms()
    {
        Debug.Log("=== TÉRMINOS RECHAZADOS ===");
        Application.Quit();
    }
} 