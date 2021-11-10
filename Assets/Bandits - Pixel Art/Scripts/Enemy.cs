using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Attributes

    public Transform player;
    public LayerMask enemyLayers;
    public float awareDistance;
    public float attackDistance;
    public float moveSpeed;
    public float shiftDistance = 0.1f;
    public int maxHealth = 100;
    public float attackRate;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 20;
    public float blockChance;
    public float minBlockTime;
    public float maxBlockTime;

    private RaycastHit2D hit;
    private Animator animator;
    private float distance = 0f;
    private float attackCooldown;
    private bool inRange;

    bool blocking;
    Bandit playerScript;
    int currentHealth;

    #endregion

    #region Initialization

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = player.gameObject.GetComponent<Bandit>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        attackCooldown = 0f;
        blocking = false;
    }

    #endregion

    #region Update Functions

    private void Update()
    {
        if (blocking) return;

        LookForPlayer();
        if (inRange)
        {
            EnemyLogic();
        }
    }

    void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackDistance)
        {
            Move();
        }
        else if (Time.time > attackCooldown)
        {
            animator.SetInteger("AnimState", 1);
            StartAttack();
            attackCooldown = Time.time + attackRate;
        }
    }

    void LookForPlayer()
    {
        if (Mathf.Abs(player.position.x - transform.position.x) <= awareDistance)
        {
            inRange = !playerScript.isDead(); // do not attack if player is dead

            if (transform.position.x > player.position.x)
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            else
                transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }
        else
            inRange = false;
    }

    #endregion

    #region Public Methods

    public void TakeDamage(int damage, bool breakStance = false)
    {
        currentHealth -= damage;
        if (breakStance)
            animator.SetTrigger("Hurt");
        if (currentHealth <= 0)
            Die();
    }

    public void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage them
        foreach (Collider2D enemy in hitEnemies)
        {
            if (playerScript.isBlocking())
            {
                if (playerScript.isParry())
                {
                    //animator.StopPlayback();
                    animator.SetTrigger("Hurt");
                    print("parry");
                    playerScript.EmitParryParticles();
                }
                else
                {
                    int direction = (transform.position.x > player.position.x) ? -1 : 1;
                    playerScript.Shift(direction);
                    playerScript.TakeDamage(Mathf.FloorToInt(attackDamage * 0.1f), attackDamage);
                    // player takes posture damage
                    playerScript.EmitParryParticles();
                }

            }
            else
                enemy.GetComponent<Bandit>().TakeDamage(attackDamage, Mathf.FloorToInt(attackDamage * 0.1f), true);
        }
    }

    public void AttemptBlock()
    {
        if (Random.Range(0f, 1f) < blockChance)
        {
            blocking = true;
            float duration = Random.Range(minBlockTime, maxBlockTime);
            animator.SetInteger("AnimState", 1);
            StartCoroutine(EnterBlockingState(Time.time, duration));
        }
    }

    public bool isBlocking()
    {
        return blocking;
    }

    public void Shift(int direction)
    {
        transform.position = new Vector3(transform.position.x + (direction * shiftDistance), transform.position.y, transform.position.z);
    }

    #endregion

    #region Private Methods

    void Die()
    {
        animator.SetTrigger("Death");
        this.GetComponent<BoxCollider2D>().enabled = false;
        this.enabled = false;
    }

    void Move()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            animator.SetInteger("AnimState", 2);
            Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    void StartAttack()
    {
        //animator.SetInteger("AnimState", 0);
        animator.SetTrigger("Attack");
    }

    IEnumerator EnterBlockingState(float startTime, float duration)
    {
        while (Time.time - startTime < duration)
            yield return null;
        blocking = false;
        animator.SetInteger("AnimState", 0);
    }

    //void AE_footstep()
    //{ //how to manage other player sounds? Add all of same infrastructure or somehow try to share them
       // m_audioManager.PlaySound("footstep");
    //}


    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    #endregion

}
