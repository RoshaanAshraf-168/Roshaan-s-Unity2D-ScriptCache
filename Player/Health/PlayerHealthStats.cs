using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHealthStats", menuName = "ScriptableObjects/PlayerHealthStats", order = 2)]
public class PlayerHealthStats : ScriptableObject
{
    [Header("Health")]
    [Range(1f, 1000f)] public float maxHealth = 100f;

    [Header("Regeneration")]
    [Range(0.1f, 10)] public float regenRate = 1f;
    [Range(1f, 20f)] public float regenDelay = 5f;

    [Header("Invincibility")]
    public float invincibilityDuration = 2f;
}
