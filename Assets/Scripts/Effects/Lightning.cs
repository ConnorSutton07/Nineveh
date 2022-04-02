using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    bool damaged;

    private void Start()
    {
        damaged = false;
        StartCoroutine(RemoveCollider(Time.time));
    }

    IEnumerator RemoveCollider(float startTime)
    {
        while (Time.time < startTime + 0.75f) { yield return null; }
        GetComponent<BoxCollider2D>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!damaged && collision.name == "Player")
        {
            collision.GetComponent<Player>().TakeDamage(20, 0, breakStance : true);
            damaged = true;
        }
    }
}
