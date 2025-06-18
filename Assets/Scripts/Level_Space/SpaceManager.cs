using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Trench/LevelGen/SpaceManager", fileName = "New Space Manager")]
public class SpaceManager : ScriptableObject
{
    public static SpaceManager Instance;

    public List<SpaceSegment> PrefabList = new List<SpaceSegment>();
    public SpaceSegment[,,] sectorPrefabs = new SpaceSegment[0, 0, 0];
    [HideInInspector] public Vector3Int sectorAmount = Vector3Int.zero;

    public void Setup()
    {
        Instance = this;
        Vector3Int min = Vector3Int.zero;
        Vector3Int max = Vector3Int.zero;

        foreach (SpaceSegment segment in PrefabList )
        {
            min.x = Mathf.Min(min.x, segment.index.x);
            min.y = Mathf.Min(min.y, segment.index.y);
            min.z = Mathf.Min(min.z, segment.index.z);

            max.x = Mathf.Max(max.x, segment.index.x);
            max.y = Mathf.Max(max.y, segment.index.y);
            max.z = Mathf.Max(max.z, segment.index.z);
        }
        sectorAmount = (max - min) + Vector3Int.one;
        sectorPrefabs = new SpaceSegment[sectorAmount.x, sectorAmount.y, sectorAmount.z];
        foreach (SpaceSegment segment in PrefabList)
        {
            Vector3Int index = segment.index - min;
            sectorPrefabs[index.x, index.y, index.z] = segment;
        }
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
