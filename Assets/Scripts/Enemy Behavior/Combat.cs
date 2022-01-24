using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public virtual void AttackPlayer(ref Bandit playerScript, ref Transform player, ref string attackSound, ref int postureDamage) { }
}
