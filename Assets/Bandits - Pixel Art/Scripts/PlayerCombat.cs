using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Update is called once per frame
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public float attackRange  = 0.5f;
    public int   attackDamage = 20;

    public void Attack()
    {
        // Play attack animation -> Handled elsewhere 

        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage them
        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            //enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
            if (enemyScript.isBlocking())
            {
                int direction = (transform.position.x > enemy.transform.position.x) ? -1 : 1;
                enemyScript.Shift(direction);
                enemyScript.TakeDamage(Mathf.FloorToInt(attackDamage * 0.1f));
                // enemy takes posture damage
            }
            else
                enemyScript.TakeDamage(attackDamage, true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
