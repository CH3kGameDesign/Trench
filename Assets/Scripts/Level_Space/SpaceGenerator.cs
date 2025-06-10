using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceGenerator : MonoBehaviour
{
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
        GameObject GO = Instantiate(SpaceManager.Instance.sectorPrefabs[0, 0, 0].gameObject, transform);
    }

    public bool GetExitPos(out Vector3 _pos,string _id)
    {
        Vector3Int _size = SpaceManager.Instance.sectorAmount;
        for (int x = 0; x <_size.x; x++)
        {
            for (int y = 0; y < _size.y; y++)
            {
                for (int z = 0; z < _size.z; z++)
                {
                    foreach (var item in SpaceManager.Instance.sectorPrefabs[x, y, z].landingSpots)
                    {
                        if (item.landingID == _id)
                        {
                            _pos = item.transform.position;
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
