using UnityEngine;

public class ContextPanelController : MonoBehaviour
{
    public GameObject contextPanel;
    public GameObject planeSearching; // arrastra aquí el objeto PlaneSearching

    public void HidePanel()
    {
        contextPanel.SetActive(false);
        if (planeSearching != null)
            planeSearching.SetActive(true);
    }
}
