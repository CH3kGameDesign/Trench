using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/LevelGen/Placeables", fileName = "New Placeables Manager")]
public class LevelGen_Placeables : ScriptableObject
{
    public static LevelGen_Placeables Instance;
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    public List<Structure> structures = new List<Structure>();
    public List<Wall> walls = new List<Wall>();
    public List<Floor> floors = new List<Floor>();
    public List<Ceiling> ceilings = new List<Ceiling>();
    public Camera PF_Camera;
    [System.Serializable]
    public class PlaceableClass
    {
        public string name;
        public Texture2D image;
        public GameObject prefab;
        public virtual void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/LevelGen/")
        {
            if (_model != null)
            {
                GameObject GO;
                string targetPath = filePath + name.Replace("/","_") + ".png";
                GO = Instantiate(_model);
                GO.transform.position = pos;
                GO.transform.eulerAngles = rot;
                _camera.Render();
                _camera.activeTexture.SaveToFile(targetPath, SetImage);
                DestroyImmediate(GO);
            }
            else
                image = onEmpty;
        }
        public void SetImage(bool set, Texture2D _texture)
        {
            if (set)
                image = _texture;
        }
    }
    [System.Serializable]
    public class SpawnPoint : PlaceableClass
    {
        public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/LevelGen/")
        {
            filePath += "Spawns/";
            base.GenerateTexture(_camera, pos, rot, onEmpty, prefab, filePath);
        }
    }
    [System.Serializable]
    public class Structure : PlaceableClass
    {
        public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/LevelGen/")
        {
            filePath += "Structures/";
            base.GenerateTexture(_camera, pos, rot, onEmpty, prefab, filePath);
        }
    }
    [System.Serializable]
    public class Wall : PlaceableClass
    {
        public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/LevelGen/")
        {
            filePath += "Structures/";
            base.GenerateTexture(_camera, pos, rot, onEmpty, prefab, filePath);
        }
    }
    [System.Serializable]
    public class Floor : PlaceableClass
    {
        public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/LevelGen/")
        {
            filePath += "Structures/";
            base.GenerateTexture(_camera, pos, rot, onEmpty, prefab, filePath);
        }
    }
    [System.Serializable]
    public class Ceiling : PlaceableClass
    {
        public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/LevelGen/")
        {
            filePath += "Structures/";
            base.GenerateTexture(_camera, pos, rot, onEmpty, prefab, filePath);
        }
    }

    public List<PointerBuilder.PlaceItem> GetSubItemList(string _id)
    {
        List<PlaceableClass> _temp = new List<PlaceableClass>();
        switch (_id)
        {
            case "spawn":
                _temp.AddRange(spawnPoints);
                break;
            case "structure":
                _temp.AddRange(structures);
                break;
            case "wall":
                _temp.AddRange(walls);
                break;
            case "floor":
                _temp.AddRange(floors);
                break;
            case "ceiling":
                _temp.AddRange(ceilings);
                break;
            default:
                break;
        }
        return ConvertToSubItemList(_temp);
    }
    public List<PointerBuilder.PlaceItem> ConvertToSubItemList(List<PlaceableClass> _list)
    {
        List<PointerBuilder.PlaceItem> _temp = new List<PointerBuilder.PlaceItem>();
        foreach (PlaceableClass _item in _list)
        {
            PointerBuilder.PlaceItem _sub = new PointerBuilder.PlaceItem();
            _sub.name = _item.name;
            _sub.image = _item.image;
            _sub._prefab = _item.prefab;
            _temp.Add(_sub);
        }    
        return _temp;
    }
#if UNITY_EDITOR
    [ContextMenu("Tools/GenerateSprites")]
    public void GenerateSprites()
    {
        Camera _camera = Instantiate(PF_Camera);
        _camera.transform.position = Vector3.zero;
        _camera.transform.forward = Vector3.forward;
        Vector3 pos = Vector3.forward * 2;
        Vector3 rot = Vector3.zero;
        foreach (var item in spawnPoints)
            item.GenerateTexture(_camera, pos, rot);
        foreach (var item in structures)
            item.GenerateTexture(_camera, pos, rot);
        foreach (var item in walls)
            item.GenerateTexture(_camera, pos, rot);
        foreach (var item in floors)
            item.GenerateTexture(_camera, pos, rot);
        foreach (var item in ceilings)
            item.GenerateTexture(_camera, pos, rot);
        DestroyImmediate(_camera.gameObject);
        AssetDatabase.Refresh();
    }
#endif
}
