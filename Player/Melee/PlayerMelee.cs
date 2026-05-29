using UnityEngine;


public class PlayerMelee : MonoBehaviour
{   
    [SerializeField] private PlayerMeleeStats meleeStats;

    [SerializeField] private bool canMelee = true;
    [SerializeField] private Transform meleePoint;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private Animator playerAnm;

    public void Update()
    {
        if (canMelee)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                // Trigger melee attack animation
                playerAnm.SetTrigger("MeleeAttackD");
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1) && canMelee)
            {
                // Trigger melee attack animation
                playerAnm.SetTrigger("MeleeAttackK");
            }
        }
    }

    public void DamageMelee()
    {   
    
        // Get all objects in the melee radius that are on the enemy layer
        Collider2D[] enemies = Physics2D.OverlapCircleAll(meleePoint.position, meleeStats.meleeRadius, enemyLayers);

        // Cycle through enemies and apply damage and knockback
        foreach (Collider2D c in enemies)
        {
            if (c.GetComponent<EnemyBaseClass>() != null)
            {   
                Debug.Log("Hit " + c.name);
                // apply full damage but reduced knockback
                EnemyBaseClass enemy = c.GetComponent<EnemyBaseClass>();
                enemy.TakeDamage(meleeStats.meleeDamage);
                enemy.KnockBack(meleeStats.MeleeKBX, meleeStats.MeleeKBY);
            }
        }
    }

    public void KnockbackMelee()
    {   
        Collider2D[] enemies = Physics2D.OverlapCircleAll(meleePoint.position, meleeStats.meleeRadius, enemyLayers);

        // Cycle through enemies and apply damage and knockback
        foreach (Collider2D c in enemies)
        {
            if (c.GetComponent<EnemyBaseClass>() != null)
            {
                // apply reduced damage but full knockback
                EnemyBaseClass enemy = c.GetComponent<EnemyBaseClass>();
                enemy.TakeDamage(meleeStats.meleeDamage * meleeStats.KnockbackMeleeDamageFactor);
                enemy.KnockBack(meleeStats.KnockbackKBX, meleeStats.KnockbackKBY);
            }
        }
    }

    // For enabling / disabling melee externally through animation events
    public void EnableMelee() { canMelee = true; }
    public void DisableMelee() { canMelee = false; }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleePoint.position, meleeStats.meleeRadius);
    }

}
