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
    [SerializeField] GameObject lightningObject;
    [SerializeField] Vector2 lightningEndPoints;
    [SerializeField] float lightningDuration;

    [Header("Sprite Materials")]
    [SerializeField] Material regularMaterial;
    [SerializeField] Material hurtMaterial;
    [SerializeField] float changeDelay;

    float rangedCooldown;
    Transform projectileOrigin;
    SpriteRenderer renderer;

    protected override void Start()
    {
        base.Start();
        projectileOrigin = transform.Find("ProjectileOrigin");
        rangedCooldown = 0;
        renderer = GetComponent<SpriteRenderer>();
        renderer.material = regularMaterial;
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
        PlaySound(attackSound);
        TakeDamage(0, postureDamage);
    }

    public void CheckForFollowup()
    {
        distance = Vector2.Distance(transform.position, target.position);
        if (distance <= attackDistance) { animator.SetTrigger("Attack2"); }
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
                    //attackSound = "deflect"; // should play deflect noise
                    //attackSound = "marduk_deflect";
                    playerScript.PlaySound("marduk_deflect");
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
        Vector2 targetVector = target.position - projectileOrigin.position;
        int direction = (int)transform.localScale.x;
        Quaternion arrowRotation = Quaternion.Euler(0f, 0f, direction * Vector2.Angle(targetVector, Vector2.up));
        GameObject arrow = Instantiate(projectile, projectileOrigin.position, arrowRotation);
    }

    void BeginRangedAttack()
    {
        animator.SetTrigger("Ranged");
    }

    void BeginLightningAttack()
    {
        animator.SetTrigger("Lightning");
    }

    public void LightningAttack()
    {
        GameObject lightning = Instantiate(lightningObject);
        float xPos = player.transform.position.x;
        lightning.GetComponent<LightningBolt2D>().startPoint = new Vector3(xPos, lightningEndPoints.x, 0);
        lightning.GetComponent<LightningBolt2D>().endPoint = new Vector3(xPos, lightningEndPoints.y, 0);
        StartCoroutine(DestroyLightning(lightning, Time.time, lightningDuration));
        PlaySound("lightning");
    }

    IEnumerator DestroyLightning(GameObject lightning, float startTime, float delay)
    {
        while (Time.time < startTime + delay) { yield return null; }
        Destroy(lightning);
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
        if (healthDamage > 0)
        {
            renderer.material = hurtMaterial;
            StartCoroutine(ChangeMaterial(Time.time, regularMaterial));
        }
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

    protected override void Die()
    {
        animator.SetTrigger("Death");
        state = State.DEAD;
        gameObject.layer = Constants.DEAD_LAYER;
    }

    public void Disable()
    {
        this.enabled = false;
    }

    IEnumerator ChangeMaterial(float startTime, Material material)
    {
        while (Time.time < startTime + changeDelay) { yield return null; }
        renderer.material = material;
    }

    protected override void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, target.position);
        if (distance > attackDistance)
        {
            if (Time.time > rangedCooldown)
            {
                rangedCooldown = Time.time + rangedCooldownTime;
                float rval = Random.Range(0f, 1f);
                if (rval < 0.5) { BeginLightningAttack(); }
                else { BeginRangedAttack(); }
            }
            else { Move(); }
        }
        else if (CanAttack())
        {
            animator.SetInteger("AnimState", 1);
            StartAttack();
            attackCooldown = Time.time + attackRate;
        }
        else { animator.SetInteger("AnimState", 1); }
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
