using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EOLCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
      if (collision.gameObject.layer == Constants.PLAYER_LAYER)
      {
          //call function to store the player vals
          //how to get player data/call function in player?
          //ask connor
          SceneManager.Load(SceneManager.Scene.Tower);
      }
    }
}
