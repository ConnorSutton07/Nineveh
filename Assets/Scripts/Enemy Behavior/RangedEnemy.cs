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

    public override void AttackPlayer(ref Bandit playerScript, ref Transform player, ref string attackSound, ref int postureDamage)
    {
        Vector2 targetVector = target.position - projectileOrigin.position;
        Quaternion arrowRotation = Quaternion.Euler(0f, 0f, Vector2.Angle(targetVector, Vector2.up));
        GameObject arrow = Instantiate(projectile, projectileOrigin.position, arrowRotation);
    }

    protected override void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, target.position);
        Debug.Log(distance);
        if (distance <= attackDistance && CanAttack())
        {
            animator.SetInteger("AnimState", 1);
            StartAttack();
            Debug.Log("Ranged enemy attack");
            attackCooldown = Time.time + attackRate;
        }
    }
}
