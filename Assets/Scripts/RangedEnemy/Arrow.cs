using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed;
    public float lifetime;
    public float distance;
    public int damage;
    Vector2 direction;
    public LayerMask collisionLayer;

    //public GameObject destroyEffect

    private void Start()
    {
        Invoke("DestroyArrow", lifetime);
        gameObject.layer = Constants.PROJECTILE_LAYER;
    }

    public void SetAttributes(Vector2 lineToPlayer)
    {
        

    }

    private void Update()
    {
        //want to change it such that it goes straight where aimed using transform.up 
        //for raycast and translate
        /*
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, -transform.right, distance, collisionLayer);
        if (hitInfo.collider != null)
        {
            if (hitInfo.collider.CompareTag("Player"))
            {
                Debug.Log("PLAYER MUST TAKE DAMAGE!");
                hitInfo.collider.GetComponent<Bandit>().TakeDamage(damage, 0, false);
            }
            DestroyArrow();
        }
        */
        transform.Translate(-transform.right * speed * Time.deltaTime);
    }

    private void DestroyArrow()
    {
        //Instantiate(destroyEffect, transform.position, Quaternion.identity)
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == Constants.PLAYER_LAYER)
        {
            collision.GetComponent<Bandit>().TakeDamage(damage, 0, false);
        }
        DestroyArrow();
    }
}
