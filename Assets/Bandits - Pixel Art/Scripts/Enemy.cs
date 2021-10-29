using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform raycast;
    public Transform player;
    public LayerMask enemyLayers;
    public float awareDistance;
    public float attackDistance;
    public float moveSpeed;
    public int maxHealth = 100;
    public float attackRate;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 20;

    private RaycastHit2D hit;
    private Animator animator;
    private float distance = 0f;
    private float attackCooldown;
    private bool inRange;

    Bandit playerScript;
    int currentHealth;

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
    }

    private void Update()
    {
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("Hurt");
        Debug.Log(currentHealth);
        if (currentHealth <= 0)
            Die();
    }

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
                }
                else
                {
                    int direction = (transform.position.x > player.position.x) ? -1 : 1;
                    playerScript.Shift(direction);
                    playerScript.TakeDamage(Mathf.FloorToInt(attackDamage * 0.1f));
                    // player takes posture damage
                }

            }
            else
                enemy.GetComponent<Bandit>().TakeDamage(attackDamage, true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

}
