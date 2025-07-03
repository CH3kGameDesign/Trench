using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen_Spawn : LevelGen_Object
{
    public spawnTypeEnum spawnType = spawnTypeEnum.enemy;
    public enum spawnTypeEnum { player, companion, enemy, treasure, boss};

    public GameObject PF_override;
    public AgentController.stateEnum _state = AgentController.stateEnum.unchanged;

    public Color C_Color = new Color (1,0,0,0.4f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = C_Color;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        Vector3 drawBoxVector = new Vector3(0.5f, 0.2f, 0.5f);
        Vector3 drawBoxPosition = transform.position + (transform.forward / 2.5f);

        Gizmos.matrix = Matrix4x4.TRS(drawBoxPosition, transform.rotation * Quaternion.Euler(0, 45, 0), drawBoxVector);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
