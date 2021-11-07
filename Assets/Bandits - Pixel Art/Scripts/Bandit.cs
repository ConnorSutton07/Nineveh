using UnityEngine;
using System.Collections;

public class Bandit : MonoBehaviour
{
    #region Attributes

    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float parryWindow = 0.25f;
    [SerializeField] float shiftDistance = 0.1f;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_Bandit m_groundSensor;
    private PlayerCombat Combat;
    private bool m_grounded = false;
    private bool m_combatIdle = false;
    private bool m_isDead = false;

    public GameObject healthSlider;
    public GameObject healthYellow;

    public LayerMask enemyLayer;
    public float attackRate = 0.9f;
    public int maxHealth = 100;
    float raycastLength = 2f;

    int currentHealth;
    float attackCooldown = 0f;
    Transform raycastOrigin;
    bool dead;
    bool blocking;
    float blockStart;
    private float healthpercentage = 1f;
    #endregion

    #region Initialization


    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
        Combat = this.GetComponent<PlayerCombat>();
        currentHealth = maxHealth;
        dead = false;
        blocking = false;
        blockStart = 0f;
        raycastOrigin = transform.Find("RaycastOrigin").transform;
    }

    #endregion

    #region Update

    // Update is called once per frame
    void Update()
    {
        float currentBlockFrames = blockStart;
        blockStart = 0;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < 0)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        // Move
        m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeed", m_body2d.velocity.y);

        // -- Handle Animations --
        blocking = false;
        //Attack
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown("g")) && Time.time >= attackCooldown)
        {
            m_animator.SetTrigger("Attack");
            //Combat.Attack();
            attackCooldown = Time.time + attackRate;
        }

        //Jump
        else if (Input.GetKeyDown("space") && m_grounded)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
            m_animator.SetInteger("AnimState", 2);

        //Combat Idle
        else if (Input.GetKey("b"))
        {
            blocking = true;
            //blockFrames = currentBlockFrames + 1;
            blockStart = (currentBlockFrames > 0) ? currentBlockFrames : Time.time;
            m_animator.SetInteger("AnimState", 1);
        }
        //Idle
        else
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

    private void updateHealthbar()
    {
        healthYellow.transform.localScale = new Vector3(healthpercentage, 1f, 1f);
        healthpercentage = (float)currentHealth / maxHealth;
        healthSlider.transform.localScale = new Vector3(healthpercentage, 1f, 1f);
    }

    #endregion

    #region Private Methods

    void Die()
    {
        m_animator.SetTrigger("Death");
        dead = true;
        gameObject.layer = 0;
    }

    #endregion

    #region Public Methods

    public void AlertEnemiesOfAttack()
    {
        print("Alerting");
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

    void RaycastDebugger(Vector3 dir)
    {
        Debug.DrawRay(raycastOrigin.position, dir * raycastLength, Color.white, 5f);
    }

    public void TakeDamage(int damage, bool breakStance = false)
    {
        currentHealth -= damage;
        updateHealthbar();
        if (breakStance)
            m_animator.SetTrigger("Hurt");
        if (currentHealth <= 0)
            Die();
    }

    public bool isDead()
    {
        return dead;
    }

    public bool isBlocking()
    {
        return blocking;
    }

    public bool isParry()
    {
        return (blockStart != 0 && Time.time - blockStart < parryWindow);
    }

    public void Shift(int direction)
    {
        transform.position = new Vector3(transform.position.x + (direction * shiftDistance), transform.position.y, transform.position.z);
    }

    #endregion
}
