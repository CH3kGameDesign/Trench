using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof(BoxCollider))]
public class ShipSpawn : MonoBehaviour
{
    private BoxCollider BC_bounds;
    public Vector2Int V2_spawnAmt;
    public List<Layout_Defined> layouts = new List<Layout_Defined>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BC_bounds = GetComponent<BoxCollider>();
        SpawnAll();
    }

    void SpawnAll()
    {
        if (layouts.Count == 0)
            return;
        int _amt = Random.Range(V2_spawnAmt.x, V2_spawnAmt.y);
        for (int i = 0; i < _amt; i++)
            Spawn(layouts.GetRandom());
    }

    void Spawn(Layout_Defined _GO)
    {
        Vector3 _spawn = GetSpawnLocation();

        LevelGen_Holder.Instance.CreateNew(_spawn, _GO, false);
    }

    Vector3 GetSpawnLocation ()
    {
        return BC_bounds.bounds.GetRandomPoint();
    }
}
