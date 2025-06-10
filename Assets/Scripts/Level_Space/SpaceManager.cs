using UnityEngine;

[CreateAssetMenu(menuName = "Trench/LevelGen/SpaceManager", fileName = "New Space Manager")]
public class SpaceManager : ScriptableObject
{
    public static SpaceManager Instance;

    public SpaceSegment DEBUG_gameObject;
    public SpaceSegment[,,] sectorPrefabs = new SpaceSegment[0, 0, 0];
    [HideInInspector] public Vector3Int sectorAmount = Vector3Int.zero;

    public void Setup()
    {
        Instance = this;
        sectorPrefabs = new SpaceSegment[1, 1, 1];
        sectorPrefabs[0, 0, 0] = DEBUG_gameObject;
        sectorAmount = Vector3Int.one;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
