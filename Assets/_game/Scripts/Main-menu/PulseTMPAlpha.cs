using UnityEngine;
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

        float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
        Color c = text.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        text.color = c;
    }
}
