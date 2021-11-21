using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum State
{
    DEAD,
    DEFAULT,
    BLOCKING,
    STUNNED
};

public class Bandit : MonoBehaviour
{
    #region Attributes

    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float parryWindow = 0.25f;
    [SerializeField] float shiftDistance = 0.1f;
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackRange = 0.5f;
    [SerializeField] int attackDamage = 20;
    [SerializeField] float stunnedAmplifier = 1.2f;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private AudioSource m_audioSource;
    private AudioManagerBanditScript m_audioManager;
    private Sensor_Bandit m_groundSensor;
    private SparkEffect sparkEffect;
    private bool m_grounded = false;
    private bool m_combatIdle = false;
    private bool m_isDead = false;

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
    float blockStart;
    private float healthPercentage = 1f;
    private float posturePercentage = 0f;

    // states

    State state;

    #endregion

    #region Initialization

    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_audioSource = GetComponent<AudioSource>();
        //m_audioManager = AudioManagerBanditScript.instance;
        m_audioManager = transform.Find("AudioManager").GetComponent<AudioManagerBanditScript>();
        //m_audioManager = transform.Find("AudioManager").transform;
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
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
        if (currentHealth < 0) state = State.DEAD;
        if (state == State.DEAD || state == State.STUNNED) return;
        state = State.DEFAULT;

        float currentBlockFrames = blockStart;
        blockStart = 0;
        
        if (!m_grounded && m_groundSensor.State()) // Check if character just landed on the ground
        { 
            m_grounded = true;
            PlaySound("land");
            m_animator.SetBool("Grounded", m_grounded);
        }
        
        if (m_grounded && !m_groundSensor.State()) // Check if character just started falling
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        float inputX = Input.GetAxis("Horizontal"); // -- Handle input and movement --
        if (inputX > 0) // Swap direction of sprite depending on walk direction
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < 0)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        
        m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y); // Move
        m_animator.SetFloat("AirSpeed", m_body2d.velocity.y); // Set AirSpeed in animator

        // -- Handle Animations --
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown("g")) && Time.time >= attackCooldown) //--- Attack
        {
            m_animator.SetTrigger("Attack");
            attackCooldown = Time.time + attackRate;
        }
        else if (Input.GetKeyDown("space") && m_grounded) //-------------------------------------------- Jump
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }
        else if (Mathf.Abs(inputX) > Mathf.Epsilon) //-------------------------------------------------- Run
        {
            m_animator.SetInteger("AnimState", 2);
        }
        else if (Input.GetKey("b")) //------------------------------------------------------------------ Combat Idle (used for block)
        {
            state = State.BLOCKING;
            blockStart = (currentBlockFrames > 0) ? currentBlockFrames : Time.time;
            m_animator.SetInteger("AnimState", 1);
        }
        else //----------------------------------------------------------------------------------------- Idle
        {
            m_animator.SetInteger("AnimState", 0);
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
        m_animator.SetTrigger("Death");
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

    #endregion

    #region Public Methods

    public void Attack()
    {
        // Play attack animation -> Handled elsewhere 

        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemies.Length == 0) PlaySound("sword_miss");

        // Damage them
        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            //enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
            if (enemyScript.isBlocking())
            {
                int direction = (transform.position.x > enemy.transform.position.x) ? -1 : 1;
                enemyScript.Shift(direction);
                enemyScript.TakeDamage(Mathf.FloorToInt(attackDamage * 0.1f), Mathf.FloorToInt(attackDamage * postureChipPercentage));
                PlaySound("block");
                EmitBlockParticles();
            }
            else
            {
                enemyScript.TakeDamage(attackDamage, Mathf.FloorToInt(attackDamage * 0.1f), true);
                // play hit sound
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
            m_animator.SetTrigger("Hurt");
        if (currentHealth <= 0)
            Die();
        else if (currentPosture >= postureThreshold)
        {
            m_animator.SetTrigger("Recover");
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

    public bool isParry()
    {
        return (blockStart != 0 && Time.time - blockStart < parryWindow);
    }

    public void Shift(int direction)
    {
        transform.position = new Vector3(transform.position.x + (direction * shiftDistance), transform.position.y, transform.position.z);
    }

    public void EmitBlockParticles()
    {
        sparkEffect.EmitBlockSparks();
    }

    public void EmitParryParticles()
    {
        sparkEffect.EmitParrySparks(); // i think this could just be a more intense version of the block particles
    }

    public void EmitAttackParticles()
    {
        sparkEffect.EmitAttackSparks();
    }

    void PlaySound(string text)
    {
        m_audioManager.PlaySound(text);
    }

    #endregion
}
