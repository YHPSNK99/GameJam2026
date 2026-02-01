using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject optionsPanel;

    public void OpenOptions()
    {
        menuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
}
