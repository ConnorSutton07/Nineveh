using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EOLCollider : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player playerScpt = GameObject.Find("Player").GetComponent<Player>();
      if (collision.name == "Player")
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
            case 0:
                return SceneManager.Scene.Start;
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
              return SceneManager.Scene.Arena;
            default:
              Debug.Log("ERROR");
              return SceneManager.Scene.Start;
        }
     }

    public void LoadScene(int levelNum)
    {
        GlobalDataPassing.Instance.SetCurrentLevel(levelNum);
        SceneManager.Scene scene = GetScene(levelNum);
        SceneManager.LoadImmediate(scene);
    }
}
