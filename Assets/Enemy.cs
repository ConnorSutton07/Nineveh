using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 100;
    private Animator animator;
    int currentHealth;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
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
}
