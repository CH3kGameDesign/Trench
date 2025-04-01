using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen_Block : MonoBehaviour
{
    public blockTypeEnum BlockType = blockTypeEnum.corridor;
    public enum blockTypeEnum { corridor, bridge, hangar }

    public List<Transform> T_architecture = new List<Transform>();
    public List<Bounds> B_bounds = new List<Bounds>();
    public List<entryClass> List_Entries = new List<entryClass>();
    public List<decorClass> List_Decor = new List<decorClass>();

    [System.Serializable]
    public class entryClass
    {
        public Transform transform = null;
        public Vector2Int size = Vector2Int.one;
        public entryTypeEnum type = entryTypeEnum.singleDoor;
        public bool connected = false;
    }
    public enum entryTypeEnum { singleDoor, wideDoor, vent}

    [System.Serializable]
    public class decorClass
    {
        public Transform transform = null;
        public Vector2Int size = Vector2Int.one;
        public decorTypeEnum type = decorTypeEnum.standard;
    }
    public enum decorTypeEnum { standard}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLists_Editor()
    {
        UpdateEntryList();
        UpdateDecorList();
        UpdateBoundingBox();
    }
    void UpdateEntryList()
    {
        List_Entries.Clear();
        Transform _holder = transform.GetChild(0);
        for (int i = 0; i < _holder.childCount; i++)
        {
            entryClass _temp = new entryClass();
            _temp.transform = _holder.GetChild(i);
            _temp.size = new Vector2Int(1, 2);
            _temp.type = entryTypeEnum.singleDoor;
            List_Entries.Add(_temp);
        }
    }
    void UpdateDecorList()
    {
        List_Decor.Clear();
        Transform _holder = transform.GetChild(1);
        for (int i = 0; i < _holder.childCount; i++)
        {
            decorClass _temp = new decorClass();
            _temp.transform = _holder.GetChild(i);
            _temp.size = new Vector2Int(1, 2);
            _temp.type = decorTypeEnum.standard;
            List_Decor.Add(_temp);
        }
    }

    public void UpdateBoundingBox()
    {
        //transform.position = Vector3.zero;
        B_bounds.Clear();
        T_architecture.Clear();
        for (int i = 0; i < transform.GetChild(2).childCount; i++)
            T_architecture.Add(transform.GetChild(2).GetChild(i));
        for (int i = 0; i < T_architecture.Count; i++)
        {
            Bounds _temp;
            Collider[] _collider = T_architecture[i].GetComponentsInChildren<Collider>();
            _temp = _collider[0].bounds;
            foreach (var item in _collider)
                _temp.Encapsulate(item.bounds);
            _temp.Expand(-0.5f);
            B_bounds.Add(_temp);
            BoxCollider GO = new GameObject().AddComponent<BoxCollider>();
            GO.transform.position = Vector3.zero;
            GO.center = _temp.center;
            GO.size = _temp.size;
        }
    }
}
