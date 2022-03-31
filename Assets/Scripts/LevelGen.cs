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

    private const int n_unique_sections = 8; // 6
    private List<int> RandomSectionGrabBag = Enumerable.Range(0, n_unique_sections).ToList();

    private float sectionWidth;
    private const int sectionHeight = 8;
    private float ground_left_transform;

    public GameObject tower;
    public GameObject double_tower;
    public GameObject tall_tower;
    public GameObject short_tall_tower;
    public GameObject twin_towers;
    public GameObject steps;
    public GameObject separate;
    public GameObject pentatower;

    /*
    [SerializeField]
    private GameObject watchtower;
    [SerializeField]
    private GameObject hill;
    */
    public GameObject[] MeleeEnemies;
    public GameObject BowEnemy;
    public Transform EnemyParent;
    public Transform PlatformParent;
    public Transform EndOfLevelCollider;
    public Transform SectionCollider;
    private List<Transform> SectionColliders = new List<Transform>();

    private Tilemap map;

    public Tile floorTile;
    public int floorHeight;
    public int tileSize;

    public bool onlyFloor;
    public bool spawnEnemies;
    public bool isTest;
    public int testSection;

    void Generate()
    {
        for (int i = 0; i < n_sections; i++)
        {
            Section sec;
            if (i == 0) { sec = new SimpleSection(floorHeight, MeleeEnemies, EnemyParent, false); }
            else if (isTest) { sec = GetSection(testSection); }
            else { sec = GetSection(); }
            sections.Add(sec);

            var col = Instantiate(SectionCollider, new Vector2((i * sectionWidth) - ground_left_transform, 1), Quaternion.identity);
            SectionColliders.Add(col);

            for (int j = -tileSize; j < sectionWidth; j += tileSize)
            {
                // create floor of the section
                int tileX = i * (int)sectionWidth - (int)ground_left_transform + j;
                map.SetTile(new Vector3Int(tileX + (tileSize - 1), floorHeight, 0), floorTile);
                map.SetTile(new Vector3Int(tileX + (tileSize - 1), floorHeight - tileSize, 0), floorTile);
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
            Vector3 endPosition = new Vector3((n_sections - 1) * sectionWidth + (sectionWidth / 2), 1, 0);
            EndOfLevelCollider.position = endPosition;
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        Random.State currentState = Random.state;
        map = gameObject.GetComponent<Tilemap>();
        GlobalDataPassing.Instance.ResetSections();
        sectionWidth = 2 * Camera.main.orthographicSize * Camera.main.aspect;
        ground_left_transform = sectionWidth / 2;
        Generate();
        //GameObject.Find("Player").GetComponent<Player>().ResetPosition();
        /*for (int i = 0; i < sections.Count(); i++)
        {
            Debug.Log("Section " + i + " is of type: " + sections[i].sectionType);
        }*/
    }

    private void Update()
    {
        if (GlobalDataPassing.Instance.EnemiesCleared())
        {
            if (GlobalDataPassing.Instance.GetPlayerSection() + 1 < SectionColliders.Count)
                Destroy(SectionColliders[GlobalDataPassing.Instance.GetPlayerSection() + 1].gameObject);
            GlobalDataPassing.Instance.IncrementPlayerSection();
            //GlobalDataPassing.Instance.IncrementPlayerSection(n_sections); //thought it maybe wasnt resetting to zero
        }
    }

    Section GetSection(int sectionNum = -1){
        if (sectionNum == -1)
        {
            if (RandomSectionGrabBag.Count() == 0)
            {
                foreach (int number in Enumerable.Range(0, n_unique_sections))
                {
                    RandomSectionGrabBag.Add(number);
                }
            }
            sectionNum = RandomSectionGrabBag[Random.Range(0, RandomSectionGrabBag.Count())];
        }
       
        Section selectedSection = null;

        switch (sectionNum)
        {
            case 0:
                selectedSection = new SimpleSection(TowerFloorNumber, MeleeEnemies, EnemyParent);
                break;
            case 1:
                selectedSection = new SingleTowerSection(TowerFloorNumber, tower, MeleeEnemies, BowEnemy, EnemyParent, PlatformParent);
                break;
            case 2:
                selectedSection = new DoubleTowerSection(TowerFloorNumber, double_tower, MeleeEnemies, BowEnemy, EnemyParent, PlatformParent);
                break;
            case 3:
                selectedSection = new ShortTallTowerSection(TowerFloorNumber, short_tall_tower, MeleeEnemies, BowEnemy, EnemyParent, PlatformParent);
                break;
            case 4:
                selectedSection = new TwinTowerSection(TowerFloorNumber, twin_towers, MeleeEnemies, BowEnemy, EnemyParent, PlatformParent);
                break;
            case 5:
                selectedSection = new StepsSection(TowerFloorNumber, steps, MeleeEnemies, BowEnemy, EnemyParent, PlatformParent);
                break;
            case 6:
                selectedSection = new SeparateSection(TowerFloorNumber, separate, MeleeEnemies, BowEnemy, EnemyParent, PlatformParent);
                break;
            case 7:
                selectedSection = new PentatowerSection(TowerFloorNumber, pentatower, MeleeEnemies, BowEnemy, EnemyParent, PlatformParent);
                break;
      //case 4:
      //     selectedSection = new TallTowerSection(TowerFloorNumber, tall_tower, MeleeEnemies,)
      // case 4:
      //     selectedSection =  new HeightStruggleSection(hill);
      // case 5:
      //     selectedSection =  new WatchTowerSection(watchtower);
          default:
                Debug.Log("Invalid section number generated in GetSection()");
                selectedSection = new SimpleSection(TowerFloorNumber, MeleeEnemies, EnemyParent);
                break;
        }

        RandomSectionGrabBag.Remove(sectionNum);

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
    public GameObject getRandomDifficulty(GameObject[] enemyChoices)
    {
        return enemyChoices[Random.Range(0, enemyChoices.Length)];
    }
}

public class SimpleSection : Section
{
    private GameObject[] MeleeEnemies;
    bool empty;
    public SimpleSection(int floorNum, GameObject[] meleeEnemies, Transform enemies, bool addEnemies = true)
    {
        sectionType = "Simple";
        TowerFloorNumber = floorNum;
        MeleeEnemies = meleeEnemies;
        EnemyParent = enemies;
        empty = !addEnemies;
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        return;
    }

    public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        if (empty)
        {
            GlobalDataPassing.Instance.AppendAliveEnemiesInSections(0);
            return;
        }
            float e1_x = (sectionIndex * sectionWidth) - (sectionWidth / 2);
        float e2_x = (sectionIndex * sectionWidth) - (sectionWidth / 3); 
        // spawn a melee enemy no matter what
        Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(e1_x, 2.25f), Quaternion.identity, EnemyParent);
        // maybe spawn another melee enemy
        if (ShouldSpawnEnemy())
        {
            Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(e2_x, 2.25f), Quaternion.identity, EnemyParent);
            GlobalDataPassing.Instance.AppendAliveEnemiesInSections(2);
        }
        else
        {
            GlobalDataPassing.Instance.AppendAliveEnemiesInSections(1);
        }
        return;
    }
}

public class SingleTowerSection : Section
{
    private GameObject tower;
    private GameObject[] MeleeEnemies;
    private GameObject RangedEnemy;

    private float x_transform;
    private float y_transform;
    private float tower_x;
    private float tower_y;

    public SingleTowerSection(int floorNum, GameObject tow, GameObject[] meleeEnemies, GameObject R_enemy, Transform enemies, Transform platforms)
    {
        sectionType = "SingleTower";
        TowerFloorNumber = floorNum;
        tower = tow;
        MeleeEnemies = meleeEnemies;
        RangedEnemy = R_enemy;
        EnemyParent = enemies;
        PlatformParent = platforms;
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        x_transform = 0; // sectionWidth / 2;
        y_transform = tower.GetComponent<BoxCollider2D>().bounds.size.y / 2;

        tower_x = (sectionIndex * sectionWidth) - x_transform;
        tower_y = (sectionHeight / 3) + y_transform;
        Instantiate(tower,
            new Vector2(tower_x, tower_y),
            Quaternion.identity,
            PlatformParent
        );
    }

    public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        // spawn a melee enemy no matter what
        Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(tower_x, 2.25f), Quaternion.identity, EnemyParent);
        // maybe spawn a ranged enemy
        if (ShouldSpawnEnemy())
        {
            Instantiate(RangedEnemy, new Vector2(tower_x, tower_y + 1), Quaternion.identity, EnemyParent);
            GlobalDataPassing.Instance.AppendAliveEnemiesInSections(2);
        }
        else
        {
            GlobalDataPassing.Instance.AppendAliveEnemiesInSections(1);
        }
        return;
    }
}

public class DoubleTowerSection : Section
{
    private GameObject tower;
    private GameObject[] MeleeEnemies;
    private GameObject RangedEnemy;
    private float x_transform;
    private float y_transform;
    private float tower_x;
    private float tower_y;

    public DoubleTowerSection(int floorNum, GameObject tow, GameObject[] meleeEnemies, GameObject R_enemy, Transform enemies, Transform platforms)
    {
        sectionType = "DoubleTower";
        TowerFloorNumber = floorNum;
        tower = tow;
        MeleeEnemies = meleeEnemies;
        RangedEnemy = R_enemy;
        EnemyParent = enemies;
        PlatformParent = platforms;
    }
    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        x_transform = 0; // sectionWidth / 2;
        y_transform = tower.GetComponent<BoxCollider2D>().bounds.size.y / 2;

        tower_x = (sectionIndex * sectionWidth) - x_transform;
        tower_y = (sectionHeight / 3) + y_transform;
        //platform_2_y = platform_1_y * 2;
        
        Instantiate(tower,
            new Vector2(tower_x, tower_y),
            Quaternion.identity,
            PlatformParent
        );
        /*
        Instantiate(platform,
            new Vector2(platform_2_x, platform_2_y),
            Quaternion.identity,
            PlatformParent
        );
        */
    }

    public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
    {
        // spawn a ranged enemy no matter what
        bool left = false;
        int enemies = 1;
        if (Random.value < 0.5) left = true;
        float shift = left ? -1f : 1f;
        Instantiate(RangedEnemy, new Vector2(tower_x + shift, tower_y + 1), Quaternion.identity, EnemyParent);
        
        // maybe spawn a second ranged enemy
        if (ShouldSpawnEnemy())
        {
            enemies++;
            Instantiate(RangedEnemy, new Vector2(tower_x - shift, tower_y + 1), Quaternion.identity, EnemyParent);
        }

        // maybe spawn a melee enemy
        if (ShouldSpawnEnemy())
        {
            enemies++;
            Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(tower_x, 2.25f), Quaternion.identity, EnemyParent);
        }
        GlobalDataPassing.Instance.AppendAliveEnemiesInSections(enemies);
        return;
    }
}

public class BarricadeSection : Section
{
    private GameObject barricade;
    private GameObject[] MeleeEnemies;
    private float x_transform;
    private float y_transform;
    private float barricade_x;

    public BarricadeSection(int floorNum, GameObject bar, GameObject[] meleeEnemies, Transform enemies, Transform platforms)
    {
        sectionType = "Barricade";
        TowerFloorNumber = floorNum;
        barricade = bar;
        MeleeEnemies = meleeEnemies;
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
        Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(barricade_x + 2, 2.25f), Quaternion.identity, EnemyParent); // right of barricade
                                                                                                                           // maybe spawn a another melee enemy
        if (ShouldSpawnEnemy())
        {
            Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(barricade_x - 2, 2.25f), Quaternion.identity, EnemyParent); // left of barricade
            GlobalDataPassing.Instance.AppendAliveEnemiesInSections(2);
        }
        else
        {
            GlobalDataPassing.Instance.AppendAliveEnemiesInSections(1);
        }
        return;
    }
}

public class ShortTallTowerSection : Section
{
  private GameObject tower;
  private GameObject[] MeleeEnemies;
  private GameObject RangedEnemy;

  private float x_transform;
  private float y_transform;
  private float tower_x;
  private float tower_y;

  public ShortTallTowerSection(int floorNum, GameObject tow, GameObject[] meleeEnemies, GameObject R_enemy, Transform enemies, Transform platforms)
  {
    sectionType = "ShortTall";
    TowerFloorNumber = floorNum;
    tower = tow;
    MeleeEnemies = meleeEnemies;
    RangedEnemy = R_enemy;
    EnemyParent = enemies;
    PlatformParent = platforms;
  }
  public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
  {
    //short tower
    x_transform = 0;
    y_transform = 0;
    tower_x = (sectionIndex * sectionWidth) - x_transform;
    tower_y = (sectionHeight / 3) + y_transform;
    Instantiate(tower,
        new Vector2(tower_x, tower_y),
        Quaternion.identity,
        PlatformParent
    );
  }

  public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
  {
    //spawn a melee enemy no matter what
    Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(tower_x, 2.25f), Quaternion.identity, EnemyParent);
    // maybe spawn a ranged enemy
    if (ShouldSpawnEnemy())
    {
      Instantiate(RangedEnemy, new Vector2(tower_x+2.5f, 4.0f ), Quaternion.identity, EnemyParent);
      GlobalDataPassing.Instance.AppendAliveEnemiesInSections(2);
    }
    else
    {
      GlobalDataPassing.Instance.AppendAliveEnemiesInSections(1);
    }
    return;
  }
}

public class TwinTowerSection : Section
{
  private GameObject tower;
  private GameObject[] MeleeEnemies;
  private GameObject RangedEnemy;

  private float x_transform;
  private float y_transform;
  private float tower_x;
  private float tower_y;

  public TwinTowerSection(int floorNum, GameObject tow, GameObject[] meleeEnemies, GameObject R_enemy, Transform enemies, Transform platforms)
  {
    sectionType = "TwinTower";
    TowerFloorNumber = floorNum;
    tower = tow;
    MeleeEnemies = meleeEnemies;
    RangedEnemy = R_enemy;
    EnemyParent = enemies;
    PlatformParent = platforms;
  }
  public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
  {
    //short tower
    x_transform = 0;
    y_transform = 0;
    tower_x = (sectionIndex * sectionWidth) - x_transform;
    tower_y = (sectionHeight / 3) + y_transform;
    Instantiate(tower,
        new Vector2(tower_x, tower_y),
        Quaternion.identity,
        PlatformParent
    );
  }

  public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
  {
    int enemiesSpawned = 1;
    //spawn a ranged enemy no matter what
    Instantiate(RangedEnemy, new Vector2(tower_x + 0.5f, 4.0f), Quaternion.identity, EnemyParent);
    //maybe spawn melee enemy
    if (ShouldSpawnEnemy())
    {
      Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(tower_x + 2.5f, 2.25f), Quaternion.identity, EnemyParent);
      enemiesSpawned++;
    }
    GlobalDataPassing.Instance.AppendAliveEnemiesInSections(enemiesSpawned);
    return;
  }
}

public class StepsSection : Section
{
  private GameObject tower;
  private GameObject[] MeleeEnemies;
  private GameObject RangedEnemy;

  private float x_transform;
  private float y_transform;
  private float tower_x;
  private float tower_y;

  public StepsSection(int floorNum, GameObject tow, GameObject[] meleeEnemies, GameObject R_enemy, Transform enemies, Transform platforms)
  {
    sectionType = "Steps";
    TowerFloorNumber = floorNum;
    tower = tow;
    MeleeEnemies = meleeEnemies;
    RangedEnemy = R_enemy;
    EnemyParent = enemies;
    PlatformParent = platforms;
  }
  public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
  {
    //short tower
    x_transform = 0;
    y_transform = 0;
    tower_x = (sectionIndex * sectionWidth + sectionWidth/3) - x_transform;
    tower_y = (sectionHeight / 3) + y_transform;
    Instantiate(tower,
        new Vector2(tower_x, tower_y),
        Quaternion.identity,
        PlatformParent
    );
  }

  public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
  {
    
    int enemiesSpawned = 1;
    //spawn a ranged enemy no matter what
    Instantiate(RangedEnemy, new Vector2(tower_x , 4.0f), Quaternion.identity, EnemyParent);
    //maybe spawn melee enemy
    if (ShouldSpawnEnemy())
    {
      Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(tower_x -8.0f, 2.25f), Quaternion.identity, EnemyParent);
      enemiesSpawned++;
    }
    //maybe spawn melee enemy
    if (ShouldSpawnEnemy())
    {
      Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(tower_x - 9.0f, 2.25f), Quaternion.identity, EnemyParent);
      enemiesSpawned++;
    }
    GlobalDataPassing.Instance.AppendAliveEnemiesInSections(enemiesSpawned); 
    return;
  }
}

public class SeparateSection : Section
{
  private GameObject tower;
  private GameObject[] MeleeEnemies;
  private GameObject RangedEnemy;

  private float x_transform;
  private float y_transform;
  private float tower_x;
  private float tower_y;

  public SeparateSection(int floorNum, GameObject tow, GameObject[] meleeEnemies, GameObject R_enemy, Transform enemies, Transform platforms)
  {
    sectionType = "Separate";
    TowerFloorNumber = floorNum;
    tower = tow;
    MeleeEnemies = meleeEnemies;
    RangedEnemy = R_enemy;
    EnemyParent = enemies;
    PlatformParent = platforms;
  }
  public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
  {
    //short tower
    x_transform = 0;
    y_transform = 0;
    tower_x = (sectionIndex * sectionWidth) - x_transform;
    tower_y = (sectionHeight / 3) + y_transform;
    Instantiate(tower,
        new Vector2(tower_x, tower_y),
        Quaternion.identity,
        PlatformParent
    );
  }

  public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
  {
    
    int enemiesSpawned = 1;
    //spawn a melee enemy no matter what
    Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(tower_x, 2.25f), Quaternion.identity, EnemyParent);
    GlobalDataPassing.Instance.AppendAliveEnemiesInSections(enemiesSpawned); 
    return;
  }
}

public class PentatowerSection : Section
{
  private GameObject tower;
  private GameObject[] MeleeEnemies;
  private GameObject RangedEnemy;

  private float x_transform;
  private float y_transform;
  private float tower_x;
  private float tower_y;

  public PentatowerSection(int floorNum, GameObject tow, GameObject[] meleeEnemies, GameObject R_enemy, Transform enemies, Transform platforms)
  {
    sectionType = "Pentatower";
    TowerFloorNumber = floorNum;
    tower = tow;
    MeleeEnemies = meleeEnemies;
    RangedEnemy = R_enemy;
    EnemyParent = enemies;
    PlatformParent = platforms;
  }
  public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
  {
    //short tower
    x_transform = 0;
    y_transform = 0;
    tower_x = (sectionIndex * sectionWidth + sectionWidth/4) - x_transform;
    tower_y = (sectionHeight / 3) + y_transform;
    Instantiate(tower,
        new Vector2(tower_x, tower_y),
        Quaternion.identity,
        PlatformParent
    );
  }

  public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
  {

    int enemiesSpawned = 2;
    //spawn a melee enemy no matter what
    Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(tower_x, 2.25f), Quaternion.identity, EnemyParent);
    //spawn a melee enemy no matter what
    Instantiate(getRandomDifficulty(MeleeEnemies), new Vector2(tower_x - 3.0f, 2.25f), Quaternion.identity, EnemyParent);
    // maybe spawn a ranged enemy
    if (ShouldSpawnEnemy())
    {
      Instantiate(RangedEnemy, new Vector2(tower_x - 3.0f, 6.0f), Quaternion.identity, EnemyParent);
      enemiesSpawned++;
    }
    else
      GlobalDataPassing.Instance.AppendAliveEnemiesInSections(enemiesSpawned);
    return;
  }
}
//public class WatchTowerSection : Section
//{
//    [SerializeField]
//    private GameObject watchtower;

//    private float x_transform;
//    private float y_transform;

//    public WatchTowerSection(GameObject bar)
//    {
//        sectionType = "WatchTower";
//        watchtower = bar;
//    }
//    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
//    {
//        x_transform = sectionWidth / 2;
//        y_transform = watchtower.GetComponent<SpriteRenderer>().bounds.size.y / 2;
//        Instantiate(watchtower,
//            new Vector2((sectionIndex * sectionWidth) - x_transform, ((3 * sectionHeight) / 4) + (y_transform)),
//            Quaternion.identity
//        );
//    }

//    public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
//    {
//        return;
//    }
//}

//public class HeightStruggleSection : Section
//{
//    [SerializeField]
//    private GameObject hill;

//    private float x_transform;
//    private float y_transform;

//    public HeightStruggleSection(GameObject hill_)
//    {
//        sectionType = "HeightStruggle";
//        hill = hill_;
//    }
//    public override void GenerateRoomObjects(int sectionIndex, float sectionWidth, int sectionHeight)
//    {
//        x_transform = sectionWidth / 2;
//        y_transform = hill.GetComponent<SpriteRenderer>().bounds.size.y / 2;
//        Instantiate(hill,
//            new Vector2((sectionIndex * sectionWidth) - x_transform, 0.5F), // floor is actually 0.5 :/
//            Quaternion.identity
//        );
//    }

//    public override void SpawnRoomEnemies(int sectionIndex, float sectionWidth, int sectionHeight)
//    {
//        return;
//    }
//}