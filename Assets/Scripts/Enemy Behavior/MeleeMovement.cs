using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeMovement : MonoBehaviour
{
    public float awareDistance;
    public float moveSpeed;
    float gapBetweenOtherEnemies;
    public LayerMask raycastDetectionLayers;
    Transform raycastPoint;

    void Start()
    {
        raycastPoint = transform.Find("RaycastPoint");
        
    }

    public (bool, float) FindPlayer(ref Bandit playerScript, ref Transform player, ref Animator animator, float prevDirection)
    {
        bool inRange = false;
        float direction = transform.localScale.x;
        if (Mathf.Abs(player.position.x - transform.position.x) <= awareDistance)
        {
            inRange = !playerScript.isDead(); // do not attack if player is dead

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) 
                direction = prevDirection;
            else
                direction = (transform.position.x > player.position.x) ? 1 : -1;

            transform.localScale = new Vector3(direction, 1.0f, 1.0f);
        }

        return (inRange, direction);
    }

    public bool EnemyInBetween()
    {
        gameObject.layer = Constants.IGNORE_RAYCAST_LAYER;
        Vector2 direction = new Vector2(-transform.localScale.x, 0f);
        gapBetweenOtherEnemies = Random.Range(1.0f, 1.5f);
        RaycastHit2D hit = Physics2D.Raycast(raycastPoint.position, direction, raycastDetectionLayers);
        if (hit.collider != null && hit.collider.gameObject.layer == Constants.ENEMY_LAYER) // check for enemy in between
        {
            if (hit.distance <= gapBetweenOtherEnemies && hit.collider.gameObject.layer == Constants.ENEMY_LAYER)
            {
                Debug.Log(hit.collider.gameObject.name);
                gameObject.layer = Constants.ENEMY_LAYER;
                return true;
            }
        }
        gameObject.layer = Constants.ENEMY_LAYER;
        return false;
    }

    public void MoveTowardsPlayer(ref Animator animator, ref Transform player)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            animator.SetInteger("AnimState", 2);
            Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }
}
