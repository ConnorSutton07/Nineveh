using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkEffect : MonoBehaviour
{
    Transform block;
    Transform attack;

    // Start is called before the first frame update
    void Start()
    {
        block  = transform.Find("Block");
        attack = transform.Find("Attack");
    }

    public void EmitBlockSparks()
    {
        return;
    }

    public void EmitParrySparks()
    {
        return;
    }

    public void EmitAttackSparks()
    {
        return;
    }
}
