using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHealth = 5;
    int currentHealth;

    void Awake() => currentHealth = maxHealth;

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{name} recibió {amount} daño. Vida: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"{name} murió.");
            gameObject.SetActive(false);
        }
    }
}
