using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField] GameObject projectile;
    Transform projectileOrigin;
    Transform target;

    protected override void Start()
    {
        base.Start();
        target = player.Find("RaycastOrigin");
        projectileOrigin = transform.Find("ProjectileOrigin");
    }

    public override void AttackPlayer(ref Player playerScript, ref Transform player, ref string attackSound, ref int postureDamage)
    {
        Vector2 targetVector = target.position - projectileOrigin.position;
        int direction = -1 * (int)transform.localScale.x;
        Quaternion arrowRotation = Quaternion.Euler(0f, 0f, direction * Vector2.Angle(targetVector, Vector2.up));
        GameObject arrow = Instantiate(projectile, projectileOrigin.position, arrowRotation);
    }

    protected override void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, target.position);
        if (distance <= attackDistance && CanAttack())
        {
            animator.SetInteger("AnimState", 1);
            StartAttack();
            attackCooldown = Time.time + attackRate;
        }
    }
}
