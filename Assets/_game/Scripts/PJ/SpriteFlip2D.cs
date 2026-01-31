using UnityEngine;

public class SpriteFlip2D : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void UpdateFlip(Vector2 moveDir)
    {
        if (moveDir.x > 0.1f)
            sr.flipX = true;  
        else if (moveDir.x < -0.1f)
            sr.flipX = false;  
    }
}
