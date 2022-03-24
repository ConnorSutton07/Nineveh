using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    // Start is called before the first frame update
    Transform threshold;
    Transform player;
    Transform groundSensor;
    SpriteRenderer renderer;

    void Start()
    {
        threshold = GameObject.Find("Threshold").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        groundSensor = player.Find("GroundSensor");
        renderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (groundSensor.position.y >= threshold.position.y)
        {
            renderer.sortingOrder = 1;
        }
        else
        {
            renderer.sortingOrder = -1;
        }
    }
}
