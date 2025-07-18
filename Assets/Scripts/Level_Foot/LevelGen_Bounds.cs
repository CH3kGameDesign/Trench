using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen_Bounds : MonoBehaviour
{
    public BoxCollider B_Bounds;
    [HideInInspector] public int I_roomNum = -1;
    [HideInInspector] public Vector3Int V3ID = Vector3Int.left;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(BoxCollider BC)
    {
        B_Bounds = BC;
        BC.isTrigger = true;
        gameObject.layer = 12;
    }
    public void SetID(Vector3Int _V3ID)
    {
        I_roomNum = _V3ID.y;
        V3ID = _V3ID;
    }
}
