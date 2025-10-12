using UnityEngine;

public class Spawner : MonoBehaviour
{
    public enum EggTypes
    {
        sharkEgg,
        fishEgg,
        shrimpEgg
    }

    public EggTypes eggType;

    [SerializeField]
    Sprite sharkEggSprite, fishEggSprite, shrimpEggSprite;

    [SerializeField]
    GameObject sharkPrefab, fishPrefab, shrimpPrefab;

    float hatchTimer = 5f;
    float hatchFinish = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = SetupEgg();
    }

    // Update is called once per frame
    void Update()
    {
        hatchTimer -= Time.deltaTime;
        if (hatchTimer < hatchFinish)
        {
            Instantiate(HatchEgg(), transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    Sprite SetupEgg()
    {
        switch (eggType)
        {
            case EggTypes.sharkEgg:
                return sharkEggSprite;
            case EggTypes.fishEgg:
                return fishEggSprite;
            case EggTypes.shrimpEgg:
                return shrimpEggSprite;
            default:
                return null;
        }
    }

    GameObject HatchEgg()
    {
        switch (eggType)
        {
            case EggTypes.sharkEgg:
                return sharkPrefab;
            case EggTypes.fishEgg:
                return fishPrefab;
            case EggTypes.shrimpEgg:
                return shrimpPrefab;
            default:
                return null;
        }
    }
}

