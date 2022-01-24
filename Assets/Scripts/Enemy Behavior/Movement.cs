using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public virtual (bool, float) FindPlayer(ref Bandit playerScript, ref Transform player, ref Animator animator, float prevDirection)
    {
        return (false, 0);
    }
    public virtual bool EnemyInBetween() { return false; }
    public virtual void MoveTowardsPlayer(ref Animator animator, ref Transform player) { }
}
