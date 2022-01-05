using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

enum State
{
    DEAD,
    DEFAULT,
    BLOCKING,
    STUNNED,
    DASHING,
    ATTACKING
};

public class Bandit : MonoBehaviour
{
    #region Attributes

    [SerializeField] float moveSpeed = 4.0f;
    [SerializeField] float jumpForce = 7.5f;
    [SerializeField] float deflectWindow = 0.25f;
    [SerializeField] float shiftDistance = 0.1f;
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackRange = 0.5f;
    [SerializeField] int attackDamage = 20;
    [SerializeField] float stunnedAmplifier = 1.2f;
    [SerializeField] float dashTime;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashCooldownLength;

    private Animator animator;
    private Rigidbody2D body;
    private AudioSource audioSource;
    private AudioManagerBanditScript audioManager;
    private Sensor_Bandit groundSensor;
    private SparkEffect sparkEffect;
    private bool grounded = false;
    private PlayerInput controls;
    
    public GameObject healthSlider;
    public GameObject healthYellow;
    public GameObject postureSlider;

    public LayerMask enemyLayer;
    public float attackRate = 0.9f;
    public int maxHealth = 100;
    public int postureThreshold = 100;
    public float postureChipPercentage;
    float raycastLength = 2f;

    int currentHealth;
    int currentPosture;
    float attackCooldown = 0f;
    Transform raycastOrigin;
    private float moveDirection;
    private float blockStart;
    private float dashCooldown;
    private float healthPercentage = 1f;
    private float posturePercentage = 0f;
    private float currentBlockFrames = 0f;
    private bool canDoubleJump = true;
    private bool blockInput = false;
    private bool moveInput = false;
    private bool canAirDash = true;

    State state;

    #endregion

    #region Initialization

    void Start()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        audioManager = transform.Find("AudioManager").GetComponent<AudioManagerBanditScript>();
        groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
        sparkEffect = transform.Find("SparkEffect").GetComponent<SparkEffect>();
        raycastOrigin = transform.Find("RaycastOrigin").transform;

        currentPosture = 0;
        currentHealth = maxHealth;
        state = State.DEFAULT;
        blockStart = 0f;
        updatePostureBar();
    }

    #endregion

    #region Update

    // Update is called once per frame
    void Update()
    {
        if (currentHealth < 0) Die();
        if (Suspended()) return;

        float currentBlockFrames = blockStart;
        blockStart = 0;
        
        if (!grounded && groundSensor.State()) // Check if character just landed on the ground
        { 
            grounded = true;
            canAirDash = true;
            canDoubleJump = true;
            PlaySound("land");
            animator.SetBool("Grounded", grounded);
        }
        
        if (grounded && !groundSensor.State()) // Check if character just started falling
        {
            grounded = false;
            canDoubleJump = false;
            animator.SetBool("Grounded", grounded);
        }

        if (blockInput)
        {
            if (grounded) StopMovement();
            Block(currentBlockFrames);
        }
        else if (moveInput && state == State.DEFAULT)
        {
            float unitDirection = (moveDirection / Mathf.Abs(moveDirection));
            transform.localScale = new Vector3(-unitDirection, 1.0f, 1.0f);
            body.velocity = new Vector2(unitDirection * moveSpeed, body.velocity.y);
            animator.SetInteger("AnimState", 2);
        }
        else
        {
            animator.SetInteger("AnimState", 0);
            StopMovement();
        }
    }

    private void FixedUpdate()
    {
        if (healthYellow.transform.localScale.x > healthSlider.transform.localScale.x)
        {
            healthYellow.transform.localScale = new Vector3((healthYellow.transform.localScale.x) - .01f, 1f, 1f);
        }
    }

    private void updateHealthBar()
    {
        healthYellow.transform.localScale = new Vector3(healthPercentage, 1f, 1f);
        healthPercentage = (float)currentHealth / maxHealth;
        healthSlider.transform.localScale = new Vector3(healthPercentage, 1f, 1f);
    }

    private void updatePostureBar()
    {
        posturePercentage = (float)currentPosture / postureThreshold;
        postureSlider.transform.localScale = new Vector3(posturePercentage, 1f, 1f);
    }

    #endregion

    #region Movement and Actions

    void OnMove(InputValue value)
    {
        moveDirection = value.Get<float>();
        moveInput = true;
    }

    void OnStopMoving()
    {
        moveInput = false;
    }

    void OnJump()
    {
        if (Suspended()) return;
        if (grounded)
        {
            animator.SetTrigger("Jump");
            grounded = false;
            animator.SetBool("Grounded", grounded);
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            groundSensor.Disable(0.2f);
        }
        else if (canDoubleJump)
        {
            canDoubleJump = false;
            canAirDash = true;
            animator.SetTrigger("Jump");
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            groundSensor.Disable(0.2f);
        }
    }

    void Die()
    {
        animator.SetTrigger("Death");
        state = State.DEAD;
        gameObject.layer = 0;
    }

    void OnDash()
    {
        if (CanDash())
        {
            int direction = -1 * (int)transform.localScale.x;
            float initialY = transform.position.y;
            state = State.DASHING;
            dashCooldown = Time.time + dashCooldownLength;
            gameObject.layer = Constants.GHOST_LAYER;
            if (!grounded) canAirDash = false;
            StartCoroutine(EnterDash(Time.time, direction, initialY));
        }
    }

    void OnAttack()
    {
        if (CanAttack())
        {
            state = State.ATTACKING;
            animator.SetTrigger("Attack");
            StartAttackCooldown();
        }
    }

    void OnBlock()
    {
        blockInput = true;
    }

    void OnExitBlock()
    {
        blockInput = false;
        ExitConditionally();
    }    

    void Block(float currentBlockFrames)
    {
        state = State.BLOCKING;
        blockStart = (currentBlockFrames > 0) ? currentBlockFrames : Time.time;
        animator.SetInteger("AnimState", 1);
    }

    void StopMovement()
    {
        body.velocity = new Vector2(0f, body.velocity.y);
    }

    #endregion

    #region Coroutines

    IEnumerator EnterDash(float startTime, int direction, float initialY)
    {
        while (Time.time - startTime < dashTime)
        {
            transform.position = new Vector3(transform.position.x + dashSpeed * direction, initialY);
            yield return null;
        }
        gameObject.layer = Constants.PLAYER_LAYER;
        state = State.DEFAULT;
    }

    #endregion

    #region Private Methods

    void RaycastDebugger(Vector3 dir)
    {
        Debug.DrawRay(raycastOrigin.position, dir * raycastLength, Color.white, 5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    #endregion

    #region Public Methods

    public void Attack()
    {
        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemies.Length == 0) PlaySound("sword_miss");

        // Damage them
        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();

            if (enemyScript.isBlocking() && transform.localScale.x != enemyScript.gameObject.transform.localScale.x)
            {
                if (Random.Range(0f, 1f) < 0.33f)
                {
                    StartAttackCooldown();
                    EmitDeflectedParticles();
                    enemyScript.SuccesfulDeflect();
                    PlaySound("deflect");
                }
                int direction = (transform.position.x > enemy.transform.position.x) ? -1 : 1;
                enemyScript.Shift(direction);
                enemyScript.TakeDamage(0, Mathf.FloorToInt(attackDamage * postureChipPercentage));
                PlaySound("block");
                EmitAttackParticles();
            }
            else
            {
                enemyScript.TakeDamage(attackDamage, Mathf.FloorToInt(attackDamage * 0.1f), true);
                PlaySound("sword_hit");
            }
        }
    }

    public void AlertEnemiesOfAttack()
    {
        Vector3 direction = (transform.localScale.x == -1f) ? Vector3.right : Vector3.left;
        Ray ray = new Ray(raycastOrigin.position, direction);
        //RaycastDebugger(direction);
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin.position, direction, raycastLength, enemyLayer);
        if (hit.collider != null)
        {
            Enemy enemyScript = hit.collider.GetComponent<Enemy>();
            enemyScript.AttemptBlock();
        }
    }

    public void TakeDamage(int healthDamage, int postureDamage, bool breakStance = false)
    {
        if (state == State.STUNNED) healthDamage = Mathf.FloorToInt(healthDamage * stunnedAmplifier); // extra damage if stunned
        currentHealth  -= healthDamage;
        currentPosture += postureDamage;

        updateHealthBar();
        updatePostureBar();

        if (breakStance) animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (currentPosture >= postureThreshold)
        {
            animator.SetTrigger("Recover");
            currentPosture = 0;
        }
    }

    public void StartAttackCooldown()
    {
        attackCooldown = Time.time + attackRate;
    }

    public bool isDead()
    {
        return state == State.DEAD;
    }

    public bool isBlocking()
    {
        return state == State.BLOCKING;
    }

    public bool CanAttack()
    {
        return Time.time >= attackCooldown && !Suspended();
    }

    public bool CanDash()
    {
        return Time.time >= dashCooldown && !Suspended() && (grounded || canAirDash);
    }

    public bool Suspended()
    {
        return state == State.DEAD || state == State.STUNNED || state == State.DASHING;
    }

    public bool isDeflect()
    {
        return (blockStart != 0 && Time.time - blockStart < deflectWindow);
    }

    public void Shift(int direction)
    {
        transform.position = new Vector3(transform.position.x + (direction * shiftDistance), transform.position.y, transform.position.z);
    }

    public void EmitBlockParticles()
    {
        sparkEffect.EmitBlockSparks();
    }

    public void EmitDeflectParticles()
    {
        sparkEffect.EmitDeflectSparks(); 
    }

    public void EmitAttackParticles()
    {
        sparkEffect.EmitAttackSparks();
    }

    public void EmitDeflectedParticles()
    {
        sparkEffect.EmitDeflectedSparks();
    }

    public void PlaySound(string text)
    {
        audioManager.PlaySound(text);
    }

    #endregion

    #region Trigger State

    public void EnterStun()
    {
        state = State.STUNNED;
    }

    public void EnterAttack()
    {
        state = State.ATTACKING;
    }

    public void ExitConditionally()
    {
        if (!Suspended()) state = State.DEFAULT;
    }

    public void ExitStun()
    {
        state = State.DEFAULT;
    }

    #endregion
}
