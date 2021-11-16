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
        var emit = block.GetComponent<ParticleSystem>().emission;
        emit.enabled = false;

    }

    public void EmitBlockSparks()
    {
        Debug.Log("emitting particles");
        var emit = block.GetComponent<ParticleSystem>().emission;
        emit.enabled = true;
        StartCoroutine(stopBlockSparks());
        return;
    }
    IEnumerator stopBlockSparks()
    {
        yield return new WaitForSeconds(.4f);
        var emit = block.GetComponent<ParticleSystem>().emission;
        emit.enabled = false;
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
