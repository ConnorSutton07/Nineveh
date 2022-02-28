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

    private void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    public void SetTrajectory(Vector2 newDirection)
    {
        direction = newDirection;
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
            Player playerScript = collision.GetComponent<Player>();
            playerScript.TakeDamage(damage, 0, true);
            playerScript.PlaySound("arrow_hit");
        }
        DestroyArrow();
    }
}
