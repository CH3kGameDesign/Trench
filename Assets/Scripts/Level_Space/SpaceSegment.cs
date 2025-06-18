using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class SpaceSegment : MonoBehaviour
{
    public Vector3Int index = new Vector3Int();
    public List<Space_LandingSpot> landingSpots = new List<Space_LandingSpot>();
    public List<NavMeshSurface> NM_surface = new List<NavMeshSurface>();
    public void Setup()
    {
        foreach (var item in NM_surface)
            item.BuildNavMesh();
    }
}
