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
    public int postureThreshold;
    public int parryDamagePercentage;

    private RaycastHit2D hit;
    private Animator animator;
    private float distance = 0f;
    private float attackCooldown;
    private AudioSource m_audioSource;
    private AudioManagerBanditScript m_audioManager; //use for now at least
    private bool inRange;

    State state;
    bool blocking;
    Bandit playerScript;
    int currentHealth;
    int currentPosture;

    #endregion

    #region Initialization

    private void Awake()
    {
        animator = GetComponent<Animator>();
        m_audioSource = GetComponent<AudioSource>();
        m_audioManager = transform.Find("AudioManager").GetComponent<AudioManagerBanditScript>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = player.gameObject.GetComponent<Bandit>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentPosture = 0;
        attackCooldown = 0f;
        blocking = false;
        state = State.DEFAULT;
    }

    #endregion

    #region Update Functions

    private void Update()
    {
        if (state != State.DEFAULT) return;

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

    public void TakeDamage(int healthDamage, int postureDamage, bool breakStance = false)
    {
        currentHealth -= healthDamage;
        currentPosture += postureDamage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (currentPosture >= postureThreshold)
        {
            animator.SetTrigger("Recover");
            currentPosture = 0;
        }
        else if (breakStance)
            animator.SetTrigger("Hurt");

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
                    //animator.SetTrigger("Hurt");
                    PlaySound("sword_hit"); //change sound of parry effect
                    playerScript.EmitParryParticles();
                    TakeDamage(0, parryDamagePercentage);
                }
                else
                {
                    int direction = (transform.position.x > player.position.x) ? -1 : 1;
                    playerScript.Shift(direction);
                    playerScript.TakeDamage(Mathf.FloorToInt(attackDamage * 0.1f), attackDamage);
                    PlaySound("sword_miss");
                    playerScript.EmitParryParticles();
                }
            }
            else
            {
                PlaySound("sword_hit");
                playerScript.TakeDamage(attackDamage, Mathf.FloorToInt(attackDamage * 0.1f), true);
            }
        }
    }

    public void AttemptBlock()
    {
        if (state == State.DEFAULT && Random.Range(0f, 1f) < blockChance)
        {
            state = State.BLOCKING;
            float duration = Random.Range(minBlockTime, maxBlockTime);
            animator.SetInteger("AnimState", 1);
            StartCoroutine(EnterBlockingState(Time.time, duration));
        }
    }

    IEnumerator EnterBlockingState(float startTime, float duration)
    {
        while (Time.time - startTime < duration)
            yield return null;
        if (state == State.BLOCKING)
        {
            state = State.DEFAULT;
            animator.SetInteger("AnimState", 0);
        }
    }

    public bool isBlocking()
    {
        print(state);
        return state == State.BLOCKING;
    }

    public void Shift(int direction)
    {
        transform.position = new Vector3(transform.position.x + (direction * shiftDistance), transform.position.y, transform.position.z);
    }

    public void EnterStun()
    {
        state = State.STUNNED;
    }

    public void ExitStun()
    {
        state = State.DEFAULT;
        animator.SetInteger("AnimState", 1);
    }

    #endregion

    #region Private Methods

    void Die()
    {
        state = State.DEAD;
        animator.SetTrigger("Death");
        this.GetComponent<BoxCollider2D>().enabled = false;
        // this.enabled = false;
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

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    void PlaySound(string text)
    {
        m_audioManager.PlaySound(text);
    }

    #endregion

}
