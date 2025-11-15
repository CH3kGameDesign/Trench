using System.Collections.Generic;
using UnityEngine;

public class GraffitiManager : MonoBehaviour
{
    public static GraffitiManager Instance;
    [System.Serializable]
    public class layerClass
    {
        public int _stampID;
        public Vector2Int _position;
        public Vector2Int _scale;
        public float _rotation;
        public Color _color;

        public layerClass()
        {
            _stampID = -1;
        }
    }

    [System.Serializable]
    public class graffitiClass
    {
        public string _name;
        public List<layerClass> _layers = new List<layerClass>();

        public graffitiClass()
        {
            _name = "New Graffiti";
            _layers = new List<layerClass>(18);
        }
    }

    public enum stampTypeEnum { primitives, letters, complex}
    public Stamp_List stampList;

    public List<Stamp_Scriptable> GetList(stampTypeEnum _type)
    {
        List<Stamp_Scriptable> _list = new List<Stamp_Scriptable>();
        foreach (var item in stampList.list)
        {
            if (item.stampType == _type)
                _list.Add(item);
        }
        return _list;
    }

    private void Awake()
    {
        Instance = this;
    }
}
