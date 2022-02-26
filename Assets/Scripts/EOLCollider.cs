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
          GlobalDataPassing.Instance.IncreaseLevel();
          Debug.Log("Level " + GlobalDataPassing.Instance.GetCurrentLevel());
          SceneManager.Scene scene = GetScene(GlobalDataPassing.Instance.GetCurrentLevel());
          SceneManager.Load(scene);
      }
    }

    private SceneManager.Scene GetScene(int levelNum)
     {
        switch (levelNum)
        {
            case 1:
              return SceneManager.Scene.Floor1;
            case 2:
              return SceneManager.Scene.Floor2;
            case 3:
              return SceneManager.Scene.Floor3;
            case 4:
              return SceneManager.Scene.Floor4;
            case 5:
              return SceneManager.Scene.Floor5;
            case 6:
              return SceneManager.Scene.Floor6;
            case 7:
              return SceneManager.Scene.Arena;
            default:
              Debug.Log("ERROR");
              return SceneManager.Scene.Start;
        }
     }
}
