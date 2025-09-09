using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Prefab_Environment))]
public class LevelGen_Door : LevelGen_Object
{
    public LevelGen_Block.entryTypeEnum entryType = LevelGen_Block.entryTypeEnum.singleDoor;
    public GameObject G_door;
    public GameObject G_frame;
    public bool B_showFrame = true;
    [HideInInspector] public bool B_connected = false;

    [HideInInspector] public bool B_validDoor = true;
    [HideInInspector] public LevelGen_Door LGD_connectedDoor = null;

    private Prefab_Environment prefabEnvironment;

    // Start is called before the first frame update
    public void Awake()
    {
        prefabEnvironment = GetComponent<Prefab_Environment>();
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

    public void MakeFake(RoomUpdater ru)
    {
        if (!B_validDoor)
            return;
        B_validDoor = false;
        B_connected = true;
        LGD_connectedDoor = Instantiate(this, transform.position, transform.rotation * Quaternion.Euler(new Vector3(0,180,0)));
        LGD_connectedDoor.Awake();
        LGD_connectedDoor.B_validDoor = false;
        LGD_connectedDoor.LGD_connectedDoor = this;

        float dist = Mathf.Infinity;
        SurfaceUpdater SU = null;
        foreach (var item in ru.walls)
        {
            float _dist = item.SU.mc.bounds.SqrDistance(transform.position);
            if (_dist < dist)
            {
                SU = item.SU;
                dist = _dist;
            }
        }
        if (SU != null)
        {
            LGD_connectedDoor.transform.parent = SU.transform;
            SU.AddFurniture(LGD_connectedDoor.GetComponent<Prefab_Environment>());
            SU.SortHoles();
            SU.UpdateSurface();
        }
    }
    public void MakeValid()
    {
        if (B_validDoor)
            return;
        B_validDoor = true;
        B_connected = false;
        if (LGD_connectedDoor != null)
        {
            LGD_connectedDoor.GetComponentInParent<SurfaceUpdater>().RemoveFurniture(LGD_connectedDoor.GetComponent<Prefab_Environment>());
            Destroy(LGD_connectedDoor.gameObject);
        }
        LGD_connectedDoor = null;
    }

    public void MoveConnection()
    {
        if (LGD_connectedDoor == null) return;

        LGD_connectedDoor.transform.position = transform.position;

        if (LGD_connectedDoor.prefabEnvironment)
        {
            if (LGD_connectedDoor.prefabEnvironment.SU_surface)
            {
                LGD_connectedDoor.prefabEnvironment.SU_surface.SortHoles();
                LGD_connectedDoor.prefabEnvironment.SU_surface.UpdateSurface();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (B_validDoor)
            Gizmos.color = new Color(1, 1, 0, 0.4f);
        else
            Gizmos.color = new Color(1, 0, 0, 0.4f);
        Vector3 drawBoxVector = new Vector3(0.4f, 0.2f, 1f);
        Vector3 drawBoxPosition = transform.position;

        Gizmos.matrix = Matrix4x4.TRS(drawBoxPosition, transform.rotation, drawBoxVector);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        drawBoxVector = new Vector3(0.6f, 0.2f, 0.6f);
        drawBoxPosition = transform.position+ (transform.forward/2);

        Gizmos.matrix = Matrix4x4.TRS(drawBoxPosition, transform.rotation * Quaternion.Euler(0,45,0), drawBoxVector);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        if (G_frame != null)
            G_frame.SetActive(B_showFrame);
    }
    public Vector2 GetSize()
    {
        switch (entryType)
        {
            case LevelGen_Block.entryTypeEnum.singleDoor:
                return new Vector2(3f, 3f);
            case LevelGen_Block.entryTypeEnum.wideDoor:
                return new Vector2(8f, 3f);
            case LevelGen_Block.entryTypeEnum.vent:
                return new Vector2(2f, 2f);
            default:
                return Vector2.zero;
        }
    }
}
