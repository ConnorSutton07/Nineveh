using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGen : MonoBehaviour
{
    [SerializeField]
    private int n_sections = 5;
    [SerializeField]
    private GameObject ground;
    [SerializeField]
    private GameObject platform;
    [SerializeField]
    private GameObject barricade;
    [SerializeField]
    private GameObject watchtower;
    [SerializeField]
    private GameObject hill;

    private Tilemap map;

    [SerializeField]
    private Tile floorTile;

    private List<Section> sections;
    private const int sectionWidth = 20;
    private const int sectionHeight = 10;

    public LevelGen()
    {
        
    }

    void Generate()
    {
        int ground_x_transform = 10;
        int ground_y_transform = 0; //(int)(ground.GetComponent<BoxCollider2D>().bounds.size.y * 4);

        for (int i = 0; i < n_sections; i++)
        {
            for (int j = 0; j < sectionWidth; j++)
            {
                // create floor of the section
                map.SetTile(new Vector3Int(i * sectionWidth - ground_x_transform + j, 0 - ground_y_transform, 0), floorTile);
                Debug.Log($"Tile set at {i * sectionWidth - ground_x_transform + j}, {0 - ground_y_transform}, {0}");
            }

            //Instantiate(ground, new Vector2(i * sectionWidth - ground_x_transform, 0 - ground_y_transform), Quaternion.identity);

            // create room objects based on room type
            int section_idx = Random.Range(0, sections.Count);
            //sections[section_idx].GenerateRoomObjects(i, sectionWidth, sectionHeight);
        }
    
    }
    // Start is called before the first frame update
    void Start()
    {
        sections = new List<Section>() {
            new WatchTowerSection(watchtower),
            new BarricadeSection(barricade),
            new HeightStruggleSection(hill),
            new SinglePlatformSection(platform),
            new DoublePlatformSection(platform),
            new SimpleSection()
        };
        map = gameObject.GetComponent<Tilemap>();
        Generate();
    }
}

public abstract class Section : MonoBehaviour
{
    public string sectionType;
    public abstract void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight);
}

public class SimpleSection : Section
{
    public SimpleSection()
    {
        sectionType = "Simple";
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        return;
    }
}

public class SinglePlatformSection : Section
{
    [SerializeField]
    private GameObject platform;

    private float x_transform;
    private float y_transform;

    public SinglePlatformSection(GameObject plat)
    {
        sectionType = "SinglePlatform";
        platform = plat;
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        x_transform = sectionWidth / 2;
        y_transform = platform.GetComponent<BoxCollider2D>().bounds.size.y / 2;
        Instantiate(platform,
            new Vector2(
                (sectionIndex * sectionWidth) - x_transform,
                (sectionHeight / 3) + y_transform
            ),
            Quaternion.identity
        );
    }
}

public class DoublePlatformSection : Section
{
    [SerializeField]
    private GameObject platform;

    private float x_transform;
    private float y_transform;

    public DoublePlatformSection(GameObject plat)
    {
        sectionType = "DoublePlatform";
        platform = plat;
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        x_transform = sectionWidth / 2;
        y_transform = platform.GetComponent<BoxCollider2D>().bounds.size.y / 2;
        Instantiate(platform,
            new Vector2(
                (sectionIndex * sectionWidth) - x_transform,
                (sectionHeight / 3) + y_transform
            ),
            Quaternion.identity
        );
        Instantiate(platform,
            new Vector2(
                (sectionIndex * sectionWidth) - x_transform,
                (2 * sectionHeight / 3) + y_transform
            ),
            Quaternion.identity
        );
    }
}

public class BarricadeSection : Section
{
    [SerializeField]
    private GameObject barricade;

    private float x_transform;
    private float y_transform;

    public BarricadeSection(GameObject bar)
    {
        sectionType = "Barricade";
        barricade = bar;
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        x_transform = sectionWidth / 2;
        y_transform = barricade.GetComponent<SpriteRenderer>().bounds.size.y / 2;
        Instantiate(barricade,
            new Vector2((sectionIndex * sectionWidth) - x_transform, y_transform + 0.5F), // floor is actually 0.5 :/
            Quaternion.identity
        );
    }
}

public class WatchTowerSection : Section
{
    [SerializeField]
    private GameObject watchtower;

    private float x_transform;
    private float y_transform;

    public WatchTowerSection(GameObject bar)
    {
        sectionType = "WatchTower";
        watchtower = bar;
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        x_transform = sectionWidth / 2;
        y_transform = watchtower.GetComponent<SpriteRenderer>().bounds.size.y / 2;
        Instantiate(watchtower,
            new Vector2((sectionIndex * sectionWidth) - x_transform, ((3 * sectionHeight) / 4) + (y_transform)),
            Quaternion.identity
        );
    }
}

public class HeightStruggleSection : Section
{
    [SerializeField]
    private GameObject hill;

    private float x_transform;
    private float y_transform;

    public HeightStruggleSection(GameObject hill_)
    {
        sectionType = "HeightStruggle";
        hill = hill_;
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        x_transform = sectionWidth / 2;
        y_transform = hill.GetComponent<SpriteRenderer>().bounds.size.y / 2;
        Instantiate(hill,
            new Vector2((sectionIndex * sectionWidth) - x_transform, 0.5F), // floor is actually 0.5 :/
            Quaternion.identity
        );
    }
}