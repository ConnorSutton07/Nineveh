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

    [SerializeField] protected float awareDistance;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float gap;
    [SerializeField] protected LayerMask raycastDetectionLayers;
    [SerializeField] protected int flip;
    [SerializeField] protected float deflectChance;
    protected Transform raycastPoint;
    protected Transform target;
    protected Transform player;
    protected RaycastHit2D hit;
    protected Animator animator;
    protected float distance = 0f;
    protected float attackCooldown;
    protected AudioSource audioSource;
    protected AudioManager audioManager; //use for now at least
    protected bool successfulDeflect = false;
    protected float prevDirection;

    protected State state;
    protected Player playerScript;
    protected int currentHealth;
    protected int currentPosture;


    #endregion

    #region Initialization

    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioManager = transform.Find("AudioManager").GetComponent<AudioManager>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        raycastPoint = transform.Find("RaycastPoint");
        playerScript = player.gameObject.GetComponent<Player>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        currentPosture = 0;
        attackCooldown = 0f;
        state = State.DEFAULT;
        prevDirection = transform.localScale.x * flip;
        target = player.Find("RaycastOrigin");
    }

    #endregion

    #region Update 

    void Update()
    {
        if (state != State.DEFAULT) return;
        if (FindPlayer()) EnemyLogic();
        else animator.SetInteger("AnimState", 1);
    }

    #endregion

    #region Overridden Methods

    protected virtual void EnemyLogic() { return; }
    public virtual void AttackPlayer() { }
    public virtual void MoveTowardsPlayer(ref Animator animator, ref Transform player) { return; }

    #endregion

    #region Public Methods

    public virtual void TakeDamage(int healthDamage, int postureDamage, bool breakStance = false)
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
            Debug.Log("break stance");
            animator.SetTrigger("Hurt");
            attackCooldown = 0;
        }
    }

    public bool AttempDeflect()
    {
        return (Random.Range(0f, 1f) < deflectChance);
    }

    public void Attack()
    {
        AttackPlayer();
    }

    public void AttemptBlock(bool forceSucess = false, float duration = -1f)
    {
        if (forceSucess || (canBlock && Random.Range(0f, 1f) < blockChance))
        {
            Debug.Log("Block");
            animator.SetInteger("AnimState", 1);
            state = State.BLOCKING;
            if (duration == -1) duration = Random.Range(minBlockTime, maxBlockTime);
            StartCoroutine(EnterBlockingState(Time.time, duration));
        }
    }

    public bool FindPlayer()
    {
        bool inRange = false;
        float direction = transform.localScale.x;
        if (Mathf.Abs(player.position.x - transform.position.x) <= awareDistance)
        {
            inRange = !playerScript.isDead(); // do not attack if player is dead

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                direction = prevDirection;
            else
                direction = flip * ((transform.position.x > player.position.x) ? 1 : -1);

            transform.localScale = new Vector3(direction, 1.0f, 1.0f);
        }
        prevDirection = direction;
        return inRange;
    }

    public bool EnemyInBetween()
    {
        gameObject.layer = Constants.IGNORE_RAYCAST_LAYER;
        Vector2 direction = new Vector2(-transform.localScale.x, 0f);
        RaycastHit2D hit = Physics2D.Raycast(raycastPoint.position, direction, raycastDetectionLayers);
        if (hit.collider != null && hit.collider.gameObject.layer == Constants.ENEMY_LAYER) // check for enemy in between
        {
            if (hit.distance <= gap && hit.collider.gameObject.layer == Constants.ENEMY_LAYER)
            {
                gameObject.layer = Constants.ENEMY_LAYER;
                return true;
            }
        }
        gameObject.layer = Constants.ENEMY_LAYER;
        return false;
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

    public void EnterAttack()
    {
        state = State.ATTACKING;
    }

    public void ExitState()
    {
        state = State.DEFAULT;
    }

    #endregion

    #region Protected Methods
    /*
    protected bool InRange()
    {
        bool inRange = false;
        (inRange, prevDirection) = FindPlayer(ref playerScript, ref player, ref animator, prevDirection);
        return inRange;
    }
    */
    protected void Die()
    {
        Debug.Log(gameObject.name + " died");
        animator.SetTrigger("Death");
        state = State.DEAD;
        gameObject.layer = Constants.DEAD_LAYER;
        GlobalDataPassing.Instance.DecrementEnemiesInCurrentSection();
        this.enabled = false;
    }

    protected bool CanAttack()
    {
        return (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt") 
             && Time.time > attackCooldown);
    }

    protected void Move()
    {
        MoveTowardsPlayer(ref animator, ref player);
    }

    protected void StartAttack()
    {
        animator.SetTrigger("Attack");
    }


    protected void PlaySound(string text)
    {
        audioManager.PlaySound(text);
    }

    #endregion

}
