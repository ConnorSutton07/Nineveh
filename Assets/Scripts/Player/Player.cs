using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    #region Attributes

    [Header ("Stats")]
    public int maxHealth = 100;
    public int postureThreshold = 100;
    [SerializeField] Vector2 initialPos;

    [Header ("Movement")]
    [SerializeField] float moveSpeed = 4.0f;
    [SerializeField] float jumpForce = 7.5f;
    [SerializeField] float dashTime;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashCooldownLength;
    [SerializeField] float pushbackSpeed;

    [Header ("Combat")]
    [SerializeField] int attackDamage = 20;
    [SerializeField] float attackRange = 0.5f;
    [SerializeField] float attackRate = 0.9f;
    [SerializeField] float deflectWindow = 0.25f;
    [SerializeField] float shiftDistance = 0.1f;
    [SerializeField] float stunnedAmplifier = 1.2f;
    [SerializeField] float blockCooldownLength;
    [SerializeField] float postureChipPercentage;
    [SerializeField] LayerMask enemyLayer;

    [Header ("Harmony")]
    [SerializeField] float harmonyDamageAmplifier;
    //[SerializeField] float harmonyLifestealThreshold;
    [SerializeField] float harmonyLifestealRate;
    [SerializeField] float harmonyPauseTime;
    [SerializeField] float harmonyDimishRate;
    [SerializeField] float harmonyHitGain;
    [SerializeField] float harmonyDeflectGain;
    [SerializeField] float maxHarmony;
    [SerializeField] float harmonyEmission;
    [SerializeField] ParticleSystem harmonyFog;

  //if (Time.time > lastPostureIncreaseTime + posturePauseTime) currentPosture -= postureDimishRate;
  //currentPosture = Mathf.Clamp(currentPosture, 0f, 100f);
    [Header("Posture")]
    [SerializeField] float posturePauseTime;
    [SerializeField] float postureDimishRate;

    [Header ("Objects")]
    [SerializeField] Transform attackPoint;
    [SerializeField] GameObject healthSlider;
    [SerializeField] GameObject healthYellow;
    [SerializeField] GameObject postureSlider;
    [SerializeField] GameObject pauseMenu;


    [Header("Light")]
    [SerializeField] float maxIntensity;
    [SerializeField] float intensityRate;

    private Animator animator;
    private Rigidbody2D body;
    private AudioSource audioSource;
    private AudioManager audioManager;
    private GroundSensor groundSensor;
    private SparkEffect sparkEffect;
    private bool grounded = false;
    private PlayerInput controls;
    

    float raycastLength = 2f;

    public int currentHealth;
    public float currentPosture;
    
    float lastHarmonyIncreaseTime;
    float lastPostureIncreaseTime;
    float currentHarmony;
    float attackCooldown = 0f;
    Transform raycastOrigin;
    private float moveDirection;
    private float blockStart;
    private float dashCooldown;
    private float blockEnd;
    private float healthPercentage = 1f;
    private float posturePercentage = 0f;
    private float currentBlockFrames = 0f;
    private bool canDoubleJump = true;
    private bool blockInput = false;
    private bool moveInput = false;
    private bool canAirDash = true;
    private Light spotlight;
    private Vector3 freezePosition;

    private float menuLoc; //700 is on screen
    private float menuOff;
    private float menuOn;
    private float harmonyemissionrate = 0f;
    private ParticleSystem.EmissionModule HarmonyEmit;
    State state;

  #endregion

    #region Initialization

    void Start()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        spotlight = transform.Find("Light").GetComponent<Light>();
        audioSource = GetComponent<AudioSource>();
        audioManager = transform.Find("AudioManager").GetComponent<AudioManager>();
        groundSensor = transform.Find("GroundSensor").GetComponent<GroundSensor>();
        sparkEffect = transform.Find("SparkEffect").GetComponent<SparkEffect>();
        raycastOrigin = transform.Find("RaycastOrigin").transform;
        HarmonyEmit = harmonyFog.emission;

        menuOff = Screen.width * 1.5f;
        menuOn = Screen.width / 2;
        menuLoc = menuOff;

        currentPosture = 0;
        currentHarmony = 0;
        currentHealth = maxHealth;
        state = State.DEFAULT;
        blockStart = 0f;
        blockEnd = -blockCooldownLength;
        spotlight.intensity = 0f;
        spotlight.enabled = false;
        updatePostureBar();
        ResetPosition();

        if (!GlobalDataPassing.Instance.IsFirstLevel())
        {
          //pass over player stats from previous level
          LoadPlayerData();
        }
        Time.timeScale = 1;
    }

    #endregion

    #region Update

    // Update is called once per frame
    void FixedUpdate()
    {
        if (healthYellow.transform.localScale.x > healthSlider.transform.localScale.x) // update healthbar
            healthYellow.transform.localScale = new Vector3((healthYellow.transform.localScale.x) - .01f, 1f, 1f);

        //Debug.Log("LastHarmonyIncreaseTime: " + lastHarmonyIncreaseTime);
        if (Time.time > lastHarmonyIncreaseTime + harmonyPauseTime) currentHarmony -= harmonyDimishRate;
        currentHarmony = Mathf.Clamp(currentHarmony, 0f, 100f);
        harmonyemissionrate = (currentHarmony / maxHarmony) * harmonyEmission;
        HarmonyEmit.rateOverTime = currentHarmony;

        //Debug.Log("LastPostureIncreaseTime: " + lastPostureIncreaseTime);
        if (Time.time > lastPostureIncreaseTime + posturePauseTime)
        {
          currentPosture -= postureDimishRate;
          updatePostureBar();
          }
        //if (currentPosture < 0) currentPosture = 0.0;
        currentPosture = Mathf.Clamp(currentPosture, 0f, 100f);

        if (currentHealth < 0) { Die(); }
        if (state == State.FROZEN) { transform.position = new Vector3(freezePosition.x, transform.position.y, transform.position.z); }
        if (Suspended()) { return; }

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

        if (blockInput && CanBlock())
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
            if (grounded) StopMovement();
        }

        
    }
    private void Update()
    {
        if (pauseMenu.transform.position.x != menuLoc)
        {
            if(Mathf.Abs(menuLoc- pauseMenu.transform.position.x) < 1)
            {
                pauseMenu.transform.position = new Vector3(menuLoc, pauseMenu.transform.position.y, pauseMenu.transform.position.z);
            }
            pauseMenu.transform.position = Vector3.Lerp(pauseMenu.transform.position, new Vector3(menuLoc, pauseMenu.transform.position.y, pauseMenu.transform.position.z),Time.fixedDeltaTime*1f);
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

    void OnHeal()
    {
        Debug.Log("here");
        currentHealth = maxHealth;
        updateHealthBar();
    }

    public void OnStopMoving()
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
        StartCoroutine(LoadStart(Time.time, 3f));
    }

    IEnumerator LoadStart(float startTime, float delay)
    {
        while (Time.time < startTime + delay) { yield return null; }
        SceneManager.LoadImmediate(0);
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
        if (state == State.BLOCKING)
        {
            StartBlockCooldown();
            ExitConditionally();
        }

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

    void OnPause()
    {
        if (Time.timeScale == 0)
        {
            //unpausing
            Time.timeScale = 1;
            menuLoc = menuOff;
        }
        else
        {
            //pausing
            Time.timeScale = 0;
            menuLoc = menuOn;
        }
    }
    #endregion

    #region Coroutines

    IEnumerator EnterDash(float startTime, int direction, float initialY)
    {
        while (Time.time - startTime < dashTime)
        {
            transform.position = new Vector3(transform.position.x + dashSpeed * direction * Time.deltaTime, initialY);
            yield return null;
        }
        gameObject.layer = Constants.PLAYER_LAYER;
        state = State.DEFAULT;
    }

    IEnumerator EnterPushback(float startTime, int direction, float pushbackTime)
    {
        while (Time.time - startTime < pushbackTime)
        {
            transform.position = new Vector3(transform.position.x + pushbackSpeed * direction * Time.deltaTime, transform.position.y, transform.position.z);
            yield return null;
        }
        state = State.DEFAULT;
    }

    IEnumerator IncreaseLight()
    {
        while (spotlight.intensity < maxIntensity)
        {
            spotlight.intensity += intensityRate;
            yield return new WaitForSeconds(0.01f);
        }
        spotlight.intensity = maxIntensity;
    }

    IEnumerator FadeLight()
    {
        while (spotlight.intensity > 0)
        {
            spotlight.intensity -= intensityRate;
            yield return new WaitForSeconds(0.01f);
        }
        spotlight.intensity = 0;
        spotlight.enabled = false;
    }

    IEnumerator EnterFreeze(float startTime, float duration)
    {
        while (Time.time < startTime + duration) { yield return null; }
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

    private void LoadPlayerData()
    {
        currentHealth = GlobalDataPassing.Instance.GetPlayerHealth();
        currentHarmony = GlobalDataPassing.Instance.GetPlayerHarmony();
        currentPosture = GlobalDataPassing.Instance.GetPlayerPosture();
        updateHealthBar();
        updatePostureBar();
    }

    #endregion

    #region Public Methods

    public void Attack()
    {
        // Detect enemies in range of attack
        currentHarmony = Mathf.Min(currentHarmony, maxHarmony);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemies.Length == 0) PlaySound("sword_miss");

        // Damage them
        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            float damage = attackDamage * (1 + harmonyDamageAmplifier * (currentHarmony / 100));
            if (enemyScript.isBlocking() && transform.localScale.x != enemyScript.gameObject.transform.localScale.x)
            {
                if (enemyScript.AttempDeflect())
                {
                    StartAttackCooldown();
                    EmitDeflectedParticles();
                    enemyScript.SuccesfulDeflect();
                    PlaySound("deflect");
                }
                int direction = (transform.position.x > enemy.transform.position.x) ? -1 : 1;
                enemyScript.Shift(direction);
                enemyScript.TakeDamage(0, Mathf.FloorToInt(damage * postureChipPercentage));
                PlaySound("block");
                EmitAttackParticles();
            }
            else
            {
                enemyScript.TakeDamage(Mathf.FloorToInt(damage), Mathf.FloorToInt(damage * 0.1f), true);
                //Debug.Log("Health: " + currentHealth + ", Harmony: " + currentHarmony + ", Exp health: " + currentHealth + harmonyLifestealRate * currentHarmony);
                //Debug.Log("LifeStealRate: " + harmonyLifestealRate);
                currentHealth = (int)Mathf.Min(maxHealth, currentHealth + harmonyLifestealRate * currentHarmony);
                updateHealthBar();
                //Debug.Log("Act Health: " + currentHealth);
                currentHarmony += harmonyHitGain;
                lastHarmonyIncreaseTime = Time.time;
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

    public void TakeDamage(int healthDamage, int postureDamage, bool breakStance = false, float pushBackTime = 0)
    {
        if (state == State.STUNNED) healthDamage = Mathf.FloorToInt(healthDamage * stunnedAmplifier); // extra damage if stunned
        currentHealth = Mathf.Max(0, currentHealth - healthDamage);
        currentPosture = Mathf.Min(postureThreshold, currentPosture + postureDamage);
        lastPostureIncreaseTime = Time.time;

        updateHealthBar();
        updatePostureBar();

        if (pushBackTime > 0)
        {
            state = State.DASHING;
            int direction = (int)transform.localScale.x;
            StartCoroutine(EnterPushback(Time.time, direction, pushBackTime));
        }

        if (breakStance) animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (currentPosture >= postureThreshold)
        {
            animator.SetTrigger("Recover");
            currentPosture = 0f;
        }
    }

    public void ActivateLight()
    {
        spotlight.enabled = true;
        StartCoroutine(IncreaseLight());
    }

    public void DeactivateLight()
    {
        StartCoroutine(FadeLight());
    }

    public void DisableUI()
    {
        transform.Find("UI").transform.Find("Health").gameObject.SetActive(false);
    }
    
    public void EnableUI()
    {
        transform.Find("UI").transform.Find("Health").gameObject.SetActive(true);
    }

    public void ResetPosition()
    {
        transform.position = initialPos;
    }

    public void SuccessfulDeflect()
    {
        currentHarmony += harmonyDeflectGain;
        lastHarmonyIncreaseTime = Time.time;
    }

    public void StartAttackCooldown()
    {
        attackCooldown = Time.time + attackRate;
    }

    public void StartBlockCooldown()
    {
        blockEnd = Time.time;
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

    public bool CanBlock()
    {
        return Time.time >= blockCooldownLength + blockEnd;
    }

    public bool CanDash()
    {
        return Time.time >= dashCooldown && !Suspended() && (grounded || canAirDash);
    }

    public bool Suspended()
    {
        return state == State.DEAD || state == State.STUNNED || state == State.DASHING || state == State.FROZEN;
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

    public void Freeze(float duration)
    {
        state = State.FROZEN;
        freezePosition = transform.position;
        animator.SetInteger("AnimState", 1);
        StartCoroutine(EnterFreeze(Time.time, duration));
    }

    public void PermaFreeze()
    {
        state = State.FROZEN;
    }

    public void Unfreeze()
    {
        state = State.DEFAULT;
    }

    public void ResetHarmony()
    {
        currentHarmony = 0;
    }

    public void EmitDeflectedParticles()
    {
        sparkEffect.EmitDeflectedSparks();
    }

    public void PlaySound(string text)
    {
        audioManager.PlaySound(text);
    }

    public void SavePlayer()
    {
        SaveSystem.SavePlayer(this);
    }
    public void LoadPlayer()
    {
        PlayerData data = SaveSystem.LoadPlayer();

        currentHealth = data.health;
        currentPosture = data.posture;
        updateHealthBar();
        updatePostureBar();
    }

    public void StorePlayerDataGlobal()
    {
        GlobalDataPassing.Instance.SetPlayerData(currentHealth, currentHarmony, currentPosture);
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
