using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class PulseTMPAlpha : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] float speed = 2f;
    [SerializeField] float minAlpha = 0.25f;
    [SerializeField] float maxAlpha = 1f;

    void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (text == null) return;

        // Pulsación del texto
        float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
        Color c = text.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        text.color = c;

        // Detectar cualquier tecla presionada con el nuevo Input System
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene("UIScene");
        }
    }
}
