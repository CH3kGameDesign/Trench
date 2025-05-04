using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen_Door : LevelGen_Object
{
    public LevelGen_Block.entryTypeEnum entryType = LevelGen_Block.entryTypeEnum.singleDoor;
    public GameObject G_door;
    public GameObject G_frame;
    public bool B_showFrame = true;
    [HideInInspector] public bool B_connected = false;
    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnConnect()
    {
        B_connected = true;
        openDoor(true);
    }
    public void Set()
    {
        openDoor(B_connected);
    }

    public void openDoor(bool open = true)
    {
        G_door.SetActive(!open);
    }

    private void OnDrawGizmos()
    {
        Vector3 drawBoxVector = new Vector3(0.4f, 0.2f, 1f);
        Vector3 drawBoxPosition = transform.position;

        Gizmos.matrix = Matrix4x4.TRS(drawBoxPosition, transform.rotation, drawBoxVector);
        Gizmos.color = new Color(1,1,0,0.4f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        drawBoxVector = new Vector3(0.6f, 0.2f, 0.6f);
        drawBoxPosition = transform.position+ (transform.forward/2);

        Gizmos.matrix = Matrix4x4.TRS(drawBoxPosition, transform.rotation * Quaternion.Euler(0,45,0), drawBoxVector);
        Gizmos.color = new Color(1, 1, 0, 0.4f);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        if (G_frame != null)
            G_frame.SetActive(B_showFrame);
    }
}
