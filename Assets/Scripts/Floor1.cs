using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CameraShake;

public class Floor1 : MonoBehaviour
{
    [SerializeField] PerlinShake.Params shakeParams;

    // Start is called before the first frame update
    void Start()
    {
        PerlinShake();
    }

    public void PerlinShake()
    {
        Vector3 sourcePosition = transform.position;

        // Creating new instance of a shake and registering it in the system.
        CameraShaker.Shake(new PerlinShake(shakeParams, sourcePosition: sourcePosition));
    }
}
