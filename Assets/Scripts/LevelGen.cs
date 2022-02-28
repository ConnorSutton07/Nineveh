using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGen : MonoBehaviour
{
    public int TowerFloorNumber;
    private List<Section> sections = new List<Section>();
    public int n_sections = 5;

    private const int n_unique_sections = 4; // 6
    private List<int> RandomSectionGrabBag = Enumerable.Range(0, n_unique_sections).ToList();

    private const int sectionWidth = 20;
    private const int sectionHeight = 8;
    private const int ground_left_transform = 10;

    public GameObject platform;
    public GameObject barricade;

    /*
    [SerializeField]
    private GameObject watchtower;
    [SerializeField]
    private GameObject hill;
    */
    public GameObject SwordEnemy;
    public GameObject BowEnemy;
    public Transform EnemyParent;
    public Transform PlatformParent;
    public Transform EndOfLevelCollider;

    private Tilemap map;

    public Tile floorTile;
    public int floorHeight;
    public int tileSize;

    public bool onlyFloor;
    public bool spawnEnemies;

    void Generate()
    {
        for (int i = 0; i < n_sections; i++)
        {
            Section sec = GetRandomSection();
            sections.Add(sec);

            for (int j = 0; j < sectionWidth; j += tileSize)
            {
                // create floor of the section
                map.SetTile(new Vector3Int(i * sectionWidth - ground_left_transform + j, floorHeight, 0), floorTile);
                map.SetTile(new Vector3Int(i * sectionWidth - ground_left_transform + j, floorHeight - tileSize, 0), floorTile);
            }

            // int section_idx = Random.Range(0, sections.Count);
            // create room objects based on room type
            if (!onlyFloor)
            { 
                sec.GenerateRoomObjects(i, sectionWidth, sectionHeight);            
            }

            // create enemies
            if (spawnEnemies)
            {
                sec.SpawnRoomEnemies(i, sectionWidth, sectionHeight);
            }

        }
        //set end of level collider position
        if (!onlyFloor)
        {
            Vector3 endPosition = new Vector3((n_sections - 1) * sectionWidth - (sectionWidth/5) , 1, 0);
            EndOfLevelCollider.position = endPosition;
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        Random.State currentState = Random.state;
        map = gameObject.GetComponent<Tilemap>();
        Generate();
        for (int i = 0; i < sections.Count(); i++)
        {
            Debug.Log("Section " + i + " is of type: " + sections[i].sectionType);
        }
    }

    Section GetRandomSection(){
        if (RandomSectionGrabBag.Count() == 0)
        {
            foreach (int number in Enumerable.Range(0, n_unique_sections)){
                RandomSectionGrabBag.Add(number);
            }
        }

        int randomSectionNum = RandomSectionGrabBag[Random.Range(0, RandomSectionGrabBag.Count())];
        Section selectedSection = null;

        switch (randomSectionNum)
        {
            case 0:
                selectedSection = new SimpleSection(TowerFloorNumber, SwordEnemy, EnemyParent);
                break;
            case 1:
                selectedSection = new SinglePlatformSection(TowerFloorNumber, platform, SwordEnemy, BowEnemy, EnemyParent, PlatformParent);
                break;
            case 2:
                selectedSection = new DoublePlatformSection(TowerFloorNumber, platform, SwordEnemy, BowEnemy, EnemyParent, PlatformParent);
                break;
            case 3:
                selectedSection = new BarricadeSection(TowerFloorNumber, barricade, SwordEnemy, EnemyParent, PlatformParent);
                break;
            // case 4:
            //     selectedSection =  new HeightStruggleSection(hill);
            // case 5:
            //     selectedSection =  new WatchTowerSection(watchtower);
            default:
                Debug.Log("Invalid section number generated in GetRandomSection()");
                selectedSection = new SimpleSection(TowerFloorNumber, SwordEnemy, EnemyParent);
                break;
        }

        RandomSectionGrabBag.Remove(randomSectionNum);

        return selectedSection;
    }

    
}

public abstract class Section : MonoBehaviour
{
    protected Transform EnemyParent;
    protected Transform PlatformParent;
    public int TowerFloorNumber;
    public string sectionType;
    public abstract void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight);
    public abstract void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight);
    public bool ShouldSpawnEnemy(){
        // weigh the TowerFloorNumber into the probability of spawning enemies
        // chance = 10% + (TowerFloorNumber * 10%)
        return Random.Range(0, 100) < 10 + TowerFloorNumber * 10;
    }
}

public class SimpleSection : Section
{
    private GameObject MeleeEnemy;
    public SimpleSection(int floorNum, GameObject meleeEnemy, Transform enemies)
    {
        sectionType = "Simple";
        TowerFloorNumber = floorNum;
        MeleeEnemy = meleeEnemy;
        EnemyParent = enemies;
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        return;
    }

    public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        float e1_x = (sectionIndex * sectionWidth) - (sectionWidth / 2);
        float e2_x = (sectionIndex * sectionWidth) - (sectionWidth / 3); 
        // spawn a melee enemy no matter what
        Instantiate(MeleeEnemy, new Vector2(e1_x, 0), Quaternion.identity, EnemyParent);
        // maybe spawn another melee enemy
        if (ShouldSpawnEnemy())
        {
            Instantiate(MeleeEnemy, new Vector2(e2_x, 0), Quaternion.identity, EnemyParent);
        }
        return;
    }
}

public class SinglePlatformSection : Section
{
    private GameObject platform;
    private GameObject MeleeEnemy;
    private GameObject RangedEnemy;

    private float x_transform;
    private float y_transform;
    private float plat_x;
    private float plat_y;

    public SinglePlatformSection(int floorNum, GameObject plat, GameObject M_enemy, GameObject R_enemy, Transform enemies, Transform platforms)
    {
        sectionType = "SinglePlatform";
        TowerFloorNumber = floorNum;
        platform = plat;
        MeleeEnemy = M_enemy;
        RangedEnemy = R_enemy;
        EnemyParent = enemies;
        PlatformParent = platforms;
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        x_transform = sectionWidth / 2;
        y_transform = platform.GetComponent<BoxCollider2D>().bounds.size.y / 2;

        plat_x = (sectionIndex * sectionWidth) - x_transform;
        plat_y = (sectionHeight / 3) + y_transform;
        Instantiate(platform,
            new Vector2(plat_x, plat_y),
            Quaternion.identity,
            PlatformParent
        );
    }

    public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        // spawn a melee enemy no matter what
        Instantiate(MeleeEnemy, new Vector2(plat_x, 0), Quaternion.identity, EnemyParent);
        // maybe spawn a ranged enemy
        if (ShouldSpawnEnemy())
        {
            Instantiate(RangedEnemy, new Vector2(plat_x, plat_y + 1), Quaternion.identity, EnemyParent);
        }
        return;
    }
}

public class DoublePlatformSection : Section
{
    private GameObject platform;
    private GameObject MeleeEnemy;
    private GameObject RangedEnemy;
    private float x_transform;
    private float y_transform;
    private float platform_1_x;
    private float platform_1_y;
    private float platform_2_x;
    private float platform_2_y;

    public DoublePlatformSection(int floorNum, GameObject plat, GameObject M_enemy, GameObject R_enemy, Transform enemies, Transform platforms)
    {
        sectionType = "DoublePlatform";
        TowerFloorNumber = floorNum;
        platform = plat;
        MeleeEnemy = M_enemy;
        RangedEnemy = R_enemy;
        EnemyParent = enemies;
        PlatformParent = platforms;
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        x_transform = sectionWidth / 2;
        y_transform = platform.GetComponent<BoxCollider2D>().bounds.size.y / 2;

        platform_1_x = (sectionIndex * sectionWidth) - x_transform;
        platform_1_y = (sectionHeight / 3) + y_transform;
        platform_2_x = (sectionIndex * sectionWidth) - x_transform;
        platform_2_y = platform_1_y * 2;
        
        Instantiate(platform,
            new Vector2(platform_1_x, platform_1_y),
            Quaternion.identity,
            PlatformParent
        );
        Instantiate(platform,
            new Vector2(platform_2_x, platform_2_y),
            Quaternion.identity,
            PlatformParent
        );
    }

    public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        // spawn a ranged enemy no matter what
        Instantiate(RangedEnemy, new Vector2(platform_2_x, platform_2_y + 1), Quaternion.identity, EnemyParent);
        // maybe spawn a melee enemy
        if (ShouldSpawnEnemy())
        {
            Instantiate(MeleeEnemy, new Vector2(platform_1_x, 0), Quaternion.identity, EnemyParent);
        }
        return;
    }
}

public class BarricadeSection : Section
{
    private GameObject barricade;
    private GameObject MeleeEnemy;
    private float x_transform;
    private float y_transform;
    private float barricade_x;

    public BarricadeSection(int floorNum, GameObject bar, GameObject M_enemy, Transform enemies, Transform platforms)
    {
        sectionType = "Barricade";
        TowerFloorNumber = floorNum;
        barricade = bar;
        MeleeEnemy = M_enemy;
        EnemyParent = enemies;
        PlatformParent = platforms; 
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        x_transform = sectionWidth / 2;
        y_transform = barricade.GetComponent<SpriteRenderer>().bounds.size.y / 2;

        barricade_x = (sectionIndex * sectionWidth) - x_transform;
        Instantiate(barricade,
            new Vector2(barricade_x, y_transform + 0.5F), // .5 for barricade's odd height
            Quaternion.identity,
            PlatformParent
        );
    }

    public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        // spawn a melee enemy no matter what
        Instantiate(MeleeEnemy, new Vector2(barricade_x + 2, 0), Quaternion.identity, EnemyParent); // right of barricade
        // maybe spawn a another melee enemy
        if (ShouldSpawnEnemy())
        {
            Instantiate(MeleeEnemy, new Vector2(barricade_x - 2, 0), Quaternion.identity, EnemyParent); // left of barricade
        }
        return;
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

    public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        return;
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

    public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        return;
    }
}