using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

public class LevelGeneration : MonoBehaviour
{
    private Tilemap map;
    [SerializeField] Tile tile;
    [SerializeField] int sectionCount;
    [SerializeField] float sectionWidth;

    void Start()
    {
        map = gameObject.GetComponent<Tilemap>();
        map.SetTile(new Vector3Int(5, 5, 0), tile);
        map.SetTile(new Vector3Int(10, 5, 0), tile);
        map.SetTile(new Vector3Int(-5, 0, 0), tile);
    }
}

