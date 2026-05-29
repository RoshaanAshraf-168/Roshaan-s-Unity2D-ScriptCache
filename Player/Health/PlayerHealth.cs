using UnityEngine;

public class PlayerHealth : MonoBehaviour
{   
    public PlayerHealthStats healthStats;

    [SerializeField] private float health = 100f;
    [SerializeField] private float timeSinceLastDamage = 0f;
    [SerializeField] private bool isPlayerInvincible = false;
    [SerializeField] private bool canRegenerate;

    private void Update()
    {
        UpdateTimeSinceLastDamage();
        InvincibilityTimer();
        RegenerateHealth();
    }

    public void TakeDamage(float damageAmount)
    {   
        if (isPlayerInvincible == false)
        {
            // Take Damage
            health -= damageAmount;

            // Enable invincibility frames and regeneration
            timeSinceLastDamage = 0f;
            isPlayerInvincible = true;
        }

        // check for death
        if (health <= 0)
        {
            Debug.Log("Player has died.");
            // Handle player death 
        }
    }

    // Invincibility frames
    private void InvincibilityTimer()
    {
        if (isPlayerInvincible && timeSinceLastDamage >= healthStats.invincibilityDuration)
        {
            isPlayerInvincible = false;
        }
    }

    // Regeneration
    private void RegenerateHealth()
    {
        if (timeSinceLastDamage >= healthStats.regenDelay && canRegenerate && health < healthStats.maxHealth)
        {
            health += healthStats.regenRate * Time.fixedDeltaTime;
            health = Mathf.Clamp(health, 0, healthStats.maxHealth);
        }
    }

    // Time since last damage
    private void UpdateTimeSinceLastDamage()
    {
        timeSinceLastDamage += Time.fixedDeltaTime;
    }
}
