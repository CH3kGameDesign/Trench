using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof(BoxCollider))]
public class AsteroidSpawn : MonoBehaviour
{
    private BoxCollider BC_bounds;
    public Vector2Int V2_spawnAmt;
    public List<Asteroid> asteroids = new List<Asteroid>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BC_bounds = GetComponent<BoxCollider>();
        SpawnAll();
    }

    void SpawnAll()
    {
        if (asteroids.Count == 0)
            return;
        int _amt = Random.Range(V2_spawnAmt.x, V2_spawnAmt.y);
        for (int i = 0; i < _amt; i++)
            SpawnObj(asteroids.GetRandom());
    }

    void SpawnObj(Asteroid _GO)
    {
        Vector3 _spawn = GetSpawnLocation();
        Asteroid _A = Instantiate(_GO, _spawn, Random.rotation);
        _A.transform.parent = transform;
    }

    Vector3 GetSpawnLocation ()
    {
        return BC_bounds.bounds.GetRandomPoint();
    }
}
