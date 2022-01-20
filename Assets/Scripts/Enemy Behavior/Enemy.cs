using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Attributes


    public float attackDistance;
    public float shiftDistance = 0.1f;
    public int maxHealth = 100;
    public float attackRate;
    
    public float blockChance;
    public float minBlockTime;
    public float maxBlockTime;
    public int postureThreshold;
    public bool canBlock;

    private Transform player;
    private RaycastHit2D hit;
    private Animator animator;
    private float distance = 0f;
    private float attackCooldown;
    private AudioSource m_audioSource;
    private AudioManagerBanditScript m_audioManager; //use for now at least
    private MeleeCombat combatScript;
    private MeleeMovement movementScript;
    private bool successfulDeflect = false;
    private float prevDirection;


    State state;
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
        combatScript = GetComponent<MeleeCombat>();
        movementScript = GetComponent<MeleeMovement>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentPosture = 0;
        attackCooldown = 0f;
        state = State.DEFAULT;
        prevDirection = transform.localScale.x;
    }

    #endregion

    #region Update Functions

    private void Update()
    {
        if (state != State.DEFAULT) return;
        if (InRange()) EnemyLogic();
        else animator.SetInteger("AnimState", 1);
    }

    void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, player.position);
        if (movementScript.EnemyInBetween())
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

    bool InRange()
    {
        bool inRange = false;
        (inRange, prevDirection) = movementScript.FindPlayer(ref playerScript, ref player, ref animator, prevDirection);
        return inRange;
    }

    #endregion

    #region Public Methods

    public void TakeDamage(int healthDamage, int postureDamage, bool breakStance = false)
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
        else if (breakStance || animator.GetCurrentAnimatorStateInfo(0).IsName("Recover"))
        {
            animator.SetTrigger("Hurt");
            attackCooldown = 0;
        }

    }

    public void Attack()
    {
        string attackSound = "";
        int postureDamage = 0;
        combatScript.AttackPlayer(ref playerScript, ref player, ref attackSound, ref postureDamage);
        PlaySound(attackSound);
        TakeDamage(0, postureDamage);
    }

    public void AttemptBlock(bool forceSucess = false, float duration = -1f)
    {
        if (forceSucess || (canBlock && state == State.DEFAULT && Random.Range(0f, 1f) < blockChance))
        {
            animator.SetInteger("AnimState", 1);
            state = State.BLOCKING;
            if (duration == -1) duration = Random.Range(minBlockTime, maxBlockTime);
            StartCoroutine(EnterBlockingState(Time.time, duration));
        }
    }

    IEnumerator EnterBlockingState(float startTime, float duration)
    {
        while (Time.time - startTime < duration && !successfulDeflect)
        {
            yield return null;
        }

        successfulDeflect = false;

        if (state == State.BLOCKING)
        {
            state = State.DEFAULT;
            //animator.SetInteger("AnimState", 0);
        }
    }

    public void SuccesfulDeflect()
    {
        successfulDeflect = true;
    }

    public bool isBlocking()
    {
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
        attackCooldown = Time.time + attackRate;
        AttemptBlock(true);
    }

    #endregion

    #region Private Methods

    private void Die()
    {
        animator.SetTrigger("Death");
        state = State.DEAD;
        gameObject.layer = Constants.DEAD_LAYER;
        this.enabled = false;
    }

    private bool CanAttack()
    {
        return (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt") 
             && Time.time > attackCooldown);
    }

    private void Move()
    {
        movementScript.MoveTowardsPlayer(ref animator, ref player);
    }

    private void StartAttack()
    {
        animator.SetTrigger("Attack");
    }


    private void PlaySound(string text)
    {
        m_audioManager.PlaySound(text);
    }

    #endregion

}
