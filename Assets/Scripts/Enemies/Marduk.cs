using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marduk : Enemy
{
    [Header ("General")]
    [SerializeField] LayerMask enemyLayer;

    [Header ("Melee")]
    [SerializeField] int attackDamage;
    [SerializeField] Transform attackPoint1;
    [SerializeField] float attackRange1 = 0.5f;
    [SerializeField] Transform attackPoint2;
    [SerializeField] float attackRange2 = 0.5f;
    [SerializeField] int deflectPostureDamage;
    [SerializeField] float distanceForAttackFollowup;
    [SerializeField] float followupPushbackTime;

    [Header("Ranged")]
    [SerializeField] GameObject projectile;
    [SerializeField] float rangedCooldownTime;
    float rangedCooldown;
    Transform projectileOrigin;

    protected override void Start()
    {
        base.Start();
        projectileOrigin = transform.Find("ProjectileOrigin");
        rangedCooldown = 0;
    }

    #region Attacks

    public override void AttackPlayer()
    {
        int postureDamage = 0;
        string attackSound = "";

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint1.position, attackRange1, enemyLayer);

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
                    postureDamage = deflectPostureDamage;
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
        distance = Vector2.Distance(transform.position, target.position);
        if (distance <= attackDistance)
        {
            animator.SetTrigger("Attack2");
        }
        PlaySound(attackSound);
        TakeDamage(0, postureDamage);
    }

    public void FollowupMeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint2.position, attackRange2, enemyLayer);
        int postureDamage = 0;
        string attackSound = "";

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
                    postureDamage = deflectPostureDamage;
                    playerScript.TakeDamage(0, 0, false, followupPushbackTime);
                }
                else
                {
                    int direction = (transform.position.x > player.position.x) ? -1 : 1;
                    playerScript.Shift(direction);
                    playerScript.TakeDamage(Mathf.FloorToInt(attackDamage * 0.1f), 200);
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
        distance = Vector2.Distance(transform.position, target.position);
        PlaySound(attackSound);
    }

    public void RangedAttack()
    {
        rangedCooldown = Time.time + rangedCooldownTime;
        Vector2 targetVector = target.position - projectileOrigin.position;
        int direction = (int)transform.localScale.x;
        Quaternion arrowRotation = Quaternion.Euler(0f, 0f, direction * Vector2.Angle(targetVector, Vector2.up));
        GameObject arrow = Instantiate(projectile, projectileOrigin.position, arrowRotation);
    }

    void BeginRangedAttack()
    {
        animator.SetTrigger("Ranged");
    }

    #endregion

    public bool inAttackState()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1") 
            || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack2")
            || animator.GetCurrentAnimatorStateInfo(0).IsName("Ranged");
    }

    public override void TakeDamage(int healthDamage, int postureDamage, bool breakStance = false)
    {
        currentHealth -= healthDamage;
        currentPosture += postureDamage;

        if (currentHealth <= 0 || state == State.STUNNED)
        {
            Die();
        }
        else if (currentPosture >= postureThreshold)
        {
            animator.SetTrigger("Recover");
            currentPosture = 0;
        }
    }

    protected override void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, target.position);
        if (distance > attackDistance)
        {
            if (Time.time > rangedCooldown) { BeginRangedAttack(); }
            else { Move(); }
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
        Gizmos.DrawWireSphere(attackPoint1.position, attackRange1);
        Gizmos.DrawWireSphere(attackPoint2.position, attackRange2);
    }
}
