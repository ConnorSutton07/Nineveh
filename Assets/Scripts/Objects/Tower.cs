using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    // Start is called before the first frame update
    Transform threshold;
    Transform player;
    Transform groundSensor;
    Transform leftEnd;
    Transform rightEnd;
    Renderer particleRenderer;
    SpriteRenderer renderer;

    void Start()
    {
        threshold = transform.Find("Threshold").transform;
        leftEnd = transform.Find("LeftEnd").transform;
        rightEnd = transform.Find("RightEnd").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        groundSensor = player.Find("GroundSensor");
        renderer = player.GetComponent<SpriteRenderer>();
        particleRenderer = player.transform.Find("Harmony Fog").GetComponent<ParticleSystem>().GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange())
        {
            if (groundSensor.position.y >= threshold.position.y)
            {
                renderer.sortingOrder = -2;
                particleRenderer.sortingOrder = -3;
            }
            else
            {
                renderer.sortingOrder = 2;
                particleRenderer.sortingOrder = 1;
            }
        }
    }

    bool playerInRange()
    {
        Vector3 pos = player.transform.position;
        return (pos.x >= leftEnd.position.x && pos.x <= rightEnd.position.x);
    }
}
