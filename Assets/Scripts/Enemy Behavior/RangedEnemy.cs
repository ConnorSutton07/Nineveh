using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField] GameObject projectile;

    public override void AttackPlayer(ref Bandit playerScript, ref Transform player, ref string attackSound, ref int postureDamage)
    {
        Vector2 targetVector = transform.position - player.position;
        Quaternion arrowRotation = Quaternion.Euler(0f, 0f, Vector2.Angle(targetVector, Vector2.up));
        Instantiate(projectile, transform.position, arrowRotation);
    }

    protected override void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackDistance && CanAttack())
        {
            animator.SetInteger("AnimState", 1);
            StartAttack();
            attackCooldown = Time.time + attackRate;
        }
    }
}
