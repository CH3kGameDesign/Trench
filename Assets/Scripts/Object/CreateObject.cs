using PurrNet;
using System.Collections.Generic;
using UnityEngine;

public class CreateObject : MonoBehaviour
{
    public bool CreateOnStart = false;
    [Space(10)]
    public List<GameObject> PF_prefabs = new List<GameObject>();
    public spawnTypeEnum spawnType = spawnTypeEnum.server;
    public enum spawnTypeEnum { everyone, server};
    [Space(10)]
    public bool CopyPosition = true;
    public bool CopyRotation = true;
    public bool CopyParent = true;
    [Space(10)]
    public bool DestroyOnSpawn = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (CreateOnStart)
            Create();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Create()
    {
        switch (spawnType)
        {
            case spawnTypeEnum.server:
                PlayerManager.Instance.CheckMain(Create_Object);
                break;
            default:
                Create_Object();
                break;
        }
    }
    void Create_Object()
    {
        if (spawnType == spawnTypeEnum.server &&
            !NetworkManager.main.isServer &&
            !NetworkManager.main.isHost)
        {
            if (DestroyOnSpawn)
                Destroy(gameObject);
            return;
        }

        GameObject PF = PF_prefabs.GetRandom();
        Vector3 pos = CopyPosition ? transform.position : PF.transform.position;
        Quaternion rot = CopyRotation ? transform.rotation : PF.transform.rotation;
        Transform parent = CopyParent ? transform.parent : PF.transform.parent;

        Instantiate(PF, pos, rot, parent);

        if (DestroyOnSpawn)
            Destroy(gameObject);
    }
}
