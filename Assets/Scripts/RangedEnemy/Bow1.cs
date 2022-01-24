using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow1 : MonoBehaviour
{
  public GameObject projectile;
  public float startTimeBtwShots;


  private float timeBtwShots;

  private void Update()
  {
    if (timeBtwShots <= 0)
    {
      //particle effect around the firing and potentially screen shake
      Debug.Log(transform.rotation);
      Instantiate(projectile, transform.position, transform.rotation);
      timeBtwShots = startTimeBtwShots;
    }
    else
    {
      timeBtwShots -= Time.deltaTime;
    }
  }
}
