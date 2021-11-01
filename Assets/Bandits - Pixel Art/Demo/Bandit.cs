using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bandit : MonoBehaviour
{
    #region Attributes

    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float parryWindow = 0.25f;
    [SerializeField] float shiftDistance = 0.1f;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private AudioSource m_audioSource;
    private AudioManagerBanditScript m_audioManager;
    private Sensor_Bandit m_groundSensor;
    private PlayerCombat Combat;
    private bool m_grounded = false;
    private bool m_combatIdle = false;
    private bool m_isDead = false;
    

    public float attackRate = 0.9f;
    public int maxHealth = 100;
    int currentHealth;
    float attackCooldown = 0f;
    bool dead;
    bool blocking;
    float blockStart;

    #endregion

    #region Initialization

    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_audioSource = GetComponent<AudioSource>();
        m_audioManager = AudioManagerBanditScript.instance;
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
        Combat = this.GetComponent<PlayerCombat>();
        currentHealth = maxHealth;
        dead = false;
        blocking = false;
        blockStart = 0f;
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
            AE_land();
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
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

    void AE_footstep()
    {
        Debug.Log(this);
        m_audioManager.PlaySound("footstep");
    }
    void AE_jump()
    {
        Debug.Log(this);
        m_audioManager.PlaySound("jump");
    }

    void AE_land()
    {
        m_audioManager.PlaySound("land");
    }

    #endregion
}
