using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Progress;

public class SpaceGenerator : MonoBehaviour
{
    [HideInInspector] public SpaceSegment[,,] activeSegments = new SpaceSegment[0, 0, 0];
    [HideInInspector] public Vector3Int segmentAmt = Vector3Int.zero;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SaveData.themeCurrent == Themes.themeEnum.ship)
        {
            Generate();
        }
    }

    void Generate()
    {
        SpaceSegment _prefab;
        SpaceSegment GO;
        segmentAmt = SpaceManager.Instance.sectorAmount;
        activeSegments = new SpaceSegment[segmentAmt.x, segmentAmt.y, segmentAmt.z];
        for (int x = 0; x < segmentAmt.x; x++)
        {
            for (int y = 0; y < segmentAmt.y; y++)
            {
                for (int z = 0; z < segmentAmt.z; z++)
                {
                    _prefab = SpaceManager.Instance.sectorPrefabs[x, y, z];
                    if (_prefab == null) continue;

                    GO = Instantiate(_prefab, transform);
                    GO.transform.localPosition = GO.index * 500;
                    GO.Setup();
                    activeSegments[x, y, z] = GO;
                }
            }
        }
    }

    public bool GetExitPos(out Vector3 _pos,string _id)
    {
        for (int x = 0; x < segmentAmt.x; x++)
        {
            for (int y = 0; y < segmentAmt.y; y++)
            {
                for (int z = 0; z < segmentAmt.z; z++)
                {
                    foreach (var item in activeSegments[x, y, z].landingSpots)
                    {
                        if (item.landingID == _id)
                        {
                            _pos = item.exitTransform.position;
                            return true;
                        }
                    }
                }
            }
        }
        _pos = Vector3.zero;
        return false;
    }
}
