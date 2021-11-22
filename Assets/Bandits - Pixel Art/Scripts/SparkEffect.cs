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
        var emitblock = block.GetComponent<ParticleSystem>().emission;
        emitblock.enabled = false;
        var emitattack = attack.GetComponent<ParticleSystem>().emission;
        emitattack.enabled = false;
    }

    public void EmitBlockSparks()
    {
        var emitblock = block.GetComponent<ParticleSystem>().emission;
        emitblock.enabled = true;
        StartCoroutine(stopBlockSparks());
        return;
    }
    IEnumerator stopBlockSparks()
    {
        yield return new WaitForSeconds(.4f);
        var emitblock = block.GetComponent<ParticleSystem>().emission;
        emitblock.enabled = false;
    }

    public void EmitParrySparks()
    {
        return;
    }

    public void EmitAttackSparks()
    {
        var emitattack = attack.GetComponent<ParticleSystem>().emission;
        emitattack.enabled = true;
        StartCoroutine(stopAttackSparks());
        return;
    }
    IEnumerator stopAttackSparks()
    {
        yield return new WaitForSeconds(.4f);
        var emitattack = attack.GetComponent<ParticleSystem>().emission;
        emitattack.enabled = false;
    }
}
