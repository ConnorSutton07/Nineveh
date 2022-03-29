using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField] GameObject projectile;
    Transform projectileOrigin;

    protected override void Start()
    {
        base.Start();
        projectileOrigin = transform.Find("ProjectileOrigin");
    }

    public override void AttackPlayer()
    {
        Vector2 targetVector = target.position - projectileOrigin.position;
        int direction = -1 * (int)transform.localScale.x;
        Quaternion arrowRotation = Quaternion.Euler(0f, 0f, direction * Vector2.Angle(targetVector, Vector2.up));
        GameObject arrow = Instantiate(projectile, projectileOrigin.position, arrowRotation);
        PlaySound("shoot");
    }

    protected override void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, target.position);
        if (distance <= attackDistance && CanAttack() && InSameSection(distance))
        {
            animator.SetInteger("AnimState", 1);
            StartAttack();
            attackCooldown = Time.time + attackRate;
        }
    }
}
