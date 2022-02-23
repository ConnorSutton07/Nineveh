using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDataPassing : MonoBehaviour
{

    public static GlobalDataPassing Instance;
    private bool hasChanged = false;
    int health = 100;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(health);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //control the instance of the class
    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
  }
}
