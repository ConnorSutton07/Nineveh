using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeMovement : MonoBehaviour
{
    public float awareDistance;
    public float moveSpeed;

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
