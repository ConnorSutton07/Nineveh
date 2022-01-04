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
    DASHING
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
    private PlayerControls controls;
    
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
    private float blockStart;
    private float dashCooldown;
    private float healthPercentage = 1f;
    private float posturePercentage = 0f;
    private bool canDoubleJump;

    // states

    State state;

    #endregion

    #region Initialization

    private void Awake()
    {
        controls = new PlayerControls();
    }

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
        canDoubleJump = true;
        updatePostureBar();
    }

    #endregion

    #region Update

    // Update is called once per frame
    void Update()
    {
        if (currentHealth < 0) state = State.DEAD;
        if (state == State.DEAD || state == State.STUNNED || state == State.DASHING) return;
        state = State.DEFAULT;

        float currentBlockFrames = blockStart;
        blockStart = 0;
        
        if (!grounded && groundSensor.State()) // Check if character just landed on the ground
        { 
            grounded = true;
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

        float inputX = Input.GetAxis("Horizontal"); // -- Handle input and movement --
        if (inputX > 0) // Swap direction of sprite depending on walk direction
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < 0)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        
        body.velocity = new Vector2(inputX * moveSpeed, body.velocity.y); // Move
        animator.SetFloat("AirSpeed", body.velocity.y); // Set AirSpeed in animator

        // -- Handle Animations --
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown("g")) && CanAttack()) //--- Attack
        {
            animator.SetTrigger("Attack");
            startAttackCooldown();
        }
        else if (Input.GetKeyDown("space")) // && grounded) //-------------------------------------------- Jump
        {
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
                animator.SetTrigger("Jump");
                body.velocity = new Vector2(body.velocity.x, jumpForce);
                groundSensor.Disable(0.2f);
            }

        }
        else if (Input.GetKeyDown("x") && CanDash())
        {
            Dash();
        }
        else if (Mathf.Abs(inputX) > Mathf.Epsilon) //-------------------------------------------------- Run
        {
            animator.SetInteger("AnimState", 2);
        }
        else if (Input.GetKey("b")) //------------------------------------------------------------------ Combat Idle (used for block)
        {
            state = State.BLOCKING;
            blockStart = (currentBlockFrames > 0) ? currentBlockFrames : Time.time;
            animator.SetInteger("AnimState", 1);
        }
        else //----------------------------------------------------------------------------------------- Idle
        {
            animator.SetInteger("AnimState", 0);
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

    #region Private Methods

    void Die()
    {
        animator.SetTrigger("Death");
        state = State.DEAD;
        gameObject.layer = 0;
    }

    void RaycastDebugger(Vector3 dir)
    {
        Debug.DrawRay(raycastOrigin.position, dir * raycastLength, Color.white, 5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    void Dash()
    {
        int direction = -1 * (int)transform.localScale.x;
        float initialY = transform.position.y;
        state = State.DASHING;
        dashCooldown = Time.time + dashCooldownLength;
        gameObject.layer = Constants.GHOST_LAYER;

        StartCoroutine(EnterDash(Time.time, direction, initialY));
    }

    IEnumerator EnterDash(float startTime, int direction, float initialY)
    {
        while (Time.time - startTime < dashTime)
        {
            transform.position = new Vector3(transform.position.x + dashSpeed * direction, initialY);
            yield return null;
        }
        // body.isKinematic = false;
        gameObject.layer = Constants.PLAYER_LAYER;
        state = State.DEFAULT;
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
            
            if (enemyScript.isBlocking())
            {
                if (Random.Range(0f, 1f) < 0.33f)
                {
                    startAttackCooldown();
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

        if (breakStance)
        {
            animator.SetTrigger("Hurt");
        }
            
        if (currentHealth <= 0)
            Die();
        else if (currentPosture >= postureThreshold)
        {
            animator.SetTrigger("Recover");
            currentPosture = 0;
        }
    }

    public bool isDead()
    {
        return state == State.DEAD;
    }

    public bool isBlocking()
    {
        return state == State.BLOCKING;
    }

    public void EnterStun()
    {
        state = State.STUNNED;
    }

    public void ExitStun()
    {
        state = State.DEFAULT;
    }

    public bool CanAttack()
    {
        return Time.time >= attackCooldown;
    }

    public bool CanDash()
    {
        return Time.time >= dashCooldown;
    }

    public void startAttackCooldown()
    {
        attackCooldown = Time.time + attackRate;
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
        sparkEffect.EmitDeflectSparks(); // i think this could just be a more intense version of the block particles
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
}
