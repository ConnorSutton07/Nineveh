using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform raycast;
    public LayerMask enemyLayers;
    public float raycastLength;
    public float attackDistance;
    public float moveSpeed;
    public float timer;
    public int maxHealth = 100;
    public float attackRate;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 20;

    private RaycastHit2D hit;
    private GameObject target;
    private Animator animator;
    private Vector2 direction;
    private float distance = 0f;
    private float attackCooldown;
    private bool inRange;
    
    int currentHealth;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        direction = Vector2.right;
        attackCooldown = 0f;
    }

    private void Update()
    {
        if (inRange)
        {
            hit = Physics2D.Raycast(raycast.position, direction, raycastLength, enemyLayers);
            RaycastDebugger();
        }
        if (hit.collider != null)
        {
            Debug.Log(hit.collider.name);
            EnemyLogic();
        }
    }

    void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > attackDistance)
        {
            Move();
        }
        else if (Time.time > attackCooldown)
        {
            StartAttack();
            attackCooldown = Time.time + attackRate;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Player detected");
            target = collision.gameObject;
            inRange = true;
            if (transform.position.x > collision.transform.position.x)
            {
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                direction = Vector2.left;
            }
            else
            {
                transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                direction = Vector2.right;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        animator.SetInteger("AnimState", 0);
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
            Vector2 targetPosition = new Vector2(target.transform.position.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    void StartAttack()
    {
        animator.SetInteger("AnimState", 0);
        animator.SetTrigger("Attack");
    }

    public void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage them
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Bandit>().TakeDamage(attackDamage);
        }
    }

    void RaycastDebugger()
    {
        if (distance > attackDistance)
            Debug.DrawRay(raycast.position, direction * raycastLength, Color.red);
        else if (attackDistance > distance)
            Debug.DrawRay(raycast.position, direction * raycastLength, Color.green);
        
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

}
