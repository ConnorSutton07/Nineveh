using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EOLCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
      if (collision.gameObject.layer == Constants.PLAYER_LAYER)
      {
          //call function to store the player vals
          SceneManager.Load(SceneManager.Scene.Tower);
      }
    }
}
