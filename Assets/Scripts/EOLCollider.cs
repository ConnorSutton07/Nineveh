using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EOLCollider : MonoBehaviour
{

    [SerializeField]
    public GameObject player;

    private Player playerScpt;

    void Start()
    {
      playerScpt = player.GetComponent<Player>();   
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
      if (collision.gameObject.layer == Constants.PLAYER_LAYER)
      {
          playerScpt.StorePlayerDataGlobal();
          SceneManager.Load(SceneManager.Scene.Tower);
      }
    }
}
