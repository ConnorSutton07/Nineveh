using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCombat : MonoBehaviour
{
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public Transform attackPoint;
    public int attackDamage = 20;
    public int parryDamagePercentage;

    public void AttackPlayer(ref Bandit playerScript, ref Transform player, ref string attackSound, ref int postureDamage)
    {
        //float postureDamage = 0;
        //string attackSound = "";

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        // Damage them
        if (hitEnemies.Length > 0)
        {
            if (playerScript.isBlocking())
            {
                if (playerScript.isDeflect())
                {
                    attackSound = "block"; // should play parry noise
                    playerScript.EmitDeflectParticles();
                    postureDamage = parryDamagePercentage;
                }
                else
                {
                    int direction = (transform.position.x > player.position.x) ? -1 : 1;
                    playerScript.Shift(direction);
                    playerScript.TakeDamage(Mathf.FloorToInt(attackDamage * 0.1f), attackDamage);
                    attackSound = "block"; // should play block noise
                    playerScript.EmitBlockParticles();
                }
            }
            else
            {
                attackSound = "sword_hit"; 
                playerScript.TakeDamage(attackDamage, Mathf.FloorToInt(attackDamage * 0.1f), true);
            }
        }
        else attackSound = "sword_miss";
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
