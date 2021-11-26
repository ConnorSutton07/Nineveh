using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkEffect : MonoBehaviour
{
    Transform block;
    Transform attack;

    ParticleSystem blockParticles;
    ParticleSystem attackParticles;

    public int particleCount;
    public int deflectAmplifier;

    // Start is called before the first frame update
    void Start()
    {
        block  = transform.Find("Block");
        attack = transform.Find("Attack"); 
        blockParticles = block.GetComponent<ParticleSystem>();
        attackParticles = attack.GetComponent<ParticleSystem>();
    }

    public void EmitBlockSparks()
    { 
        blockParticles.Emit(particleCount);
    }

    public void EmitDeflectSparks()
    {
        blockParticles.Emit(particleCount * deflectAmplifier);
    }

    public void EmitAttackSparks()
    {
        attackParticles.Emit(particleCount);
    }

    public void EmitDeflectedSparks()
    {
        attackParticles.Emit(particleCount * deflectAmplifier);
    }
}
