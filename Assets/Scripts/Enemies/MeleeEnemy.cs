using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public Transform attackPoint;
    public int attackDamage = 20;
    public int parryDamagePercentage;

    public override void AttackPlayer(ref Player playerScript, ref Transform player, ref string attackSound, ref int postureDamage)
    {
        //float postureDamage = 0;
        //string attackSound = "";

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        // Damage them
        if (hitEnemies.Length > 0)
        {
            if (playerScript.isBlocking() && playerScript.transform.localScale.x != transform.localScale.x)
            {
                if (playerScript.isDeflect())
                {
                    attackSound = "deflect"; // should play deflect noise
                    playerScript.SuccessfulDeflect();
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

    protected override void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, player.position);
        if (EnemyInBetween())
        {
            AttemptBlock(true, 0.25f);
        }
        else if (distance > attackDistance)
        {
            Move();
        }
        else if (CanAttack())
        {
            animator.SetInteger("AnimState", 1);
            StartAttack();
            attackCooldown = Time.time + attackRate;
        }
    }

    public override void MoveTowardsPlayer(ref Animator animator, ref Transform player)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            animator.SetInteger("AnimState", 2);
            Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
