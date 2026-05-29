using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMeleeStats", menuName = "ScriptableObjects/PlayerMeleeStats", order = 3)]
public class PlayerMeleeStats : ScriptableObject
{   
    [Header("Melee Attack")]
    [Range(0f, 1000f)] public float meleeDamage = 10f;
    [Range(0f, 10f)] public float meleeRadius = 1f;
    [Range(0f, 100f)] public float KnockbackMeleeDamageFactor = 0.5f;
    
    [Header("Knockback")]
    [Range(0f, 100f)] public float MeleeKBX = 10f;
    [Range(0f, 100f)] public float MeleeKBY = 10f;
    
    [Range(0f, 100f)] public float KnockbackKBX = 10f;
    [Range(0f, 100f)] public float KnockbackKBY = 10f;
    
    
}
