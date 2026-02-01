using UnityEngine;

public class DamageZone2D : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float damageCooldown = 0.7f;

    float nextDamageTime = 0f;

    void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextDamageTime) return;

        if (other.CompareTag("Player"))
        {
            Health hp = other.GetComponent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                nextDamageTime = Time.time + damageCooldown;
            }
        }
    }
}
