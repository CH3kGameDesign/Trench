using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/LevelGen/Materials", fileName = "New Material Manager")]
public class LevelGen_Materials : ScriptableObject
{
    public static LevelGen_Materials Instance;
    public List<MaterialClass> walls = new List<MaterialClass>();
    public List<MaterialClass> floors = new List<MaterialClass>();
    public List<MaterialClass> ceilings = new List<MaterialClass>();
    public GameObject PF_sphere;
    public Camera PF_Camera;
    [System.Serializable]
    public class MaterialClass
    {
        public string name;
        public Texture2D image;
        public Material material;
        public virtual void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/LevelGen/")
        {
            GameObject GO;
            filePath += "Material/";
            string targetPath = filePath + name.Replace("/", "_") + ".png";
            GO = Instantiate(_model);
            GO.GetComponent<MeshRenderer>().material = material;
            GO.transform.position = pos;
            _camera.Render();
            _camera.activeTexture.SaveToFile(targetPath, SetImage);
            DestroyImmediate(GO);
        }
        public void SetImage(bool set, Texture2D _texture)
        {
            if (set)
                image = _texture;
        }
    }
    public List<PointerBuilder.SubItem> GetSubItemList(string _id)
    {
        List<MaterialClass> _temp = new List<MaterialClass>();
        switch (_id)
        {
            case "room":
                _temp.Concat<MaterialClass>(walls);
                _temp.Concat<MaterialClass>(floors);
                _temp.Concat<MaterialClass>(ceilings);
                break;
            case "wall":
                _temp.Concat<MaterialClass>(walls);
                break;
            case "floor":
                _temp.Concat<MaterialClass>(floors);
                break;
            case "ceiling":
                _temp.Concat<MaterialClass>(ceilings);
                break;
            default:
                break;
        }
        return ConvertToSubItemList(_temp);
    }
    public List<PointerBuilder.SubItem> ConvertToSubItemList(List<MaterialClass> _list)
    {
        List<PointerBuilder.SubItem> _temp = new List<PointerBuilder.SubItem>();
        foreach (MaterialClass _item in _list)
        {
            PointerBuilder.SubItem _sub = new PointerBuilder.SubItem();
            _sub.name = _item.name;
            _sub.image = _item.image;
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
        Vector3 pos = Vector3.forward * 6;
        Vector3 rot = Vector3.zero;
        foreach (var item in walls)
            item.GenerateTexture(_camera, pos, rot, null, PF_sphere);
        foreach (var item in floors)
            item.GenerateTexture(_camera, pos, rot, null, PF_sphere);
        foreach (var item in ceilings)
            item.GenerateTexture(_camera, pos, rot, null, PF_sphere);
        DestroyImmediate(_camera.gameObject);
        AssetDatabase.Refresh();
    }
#endif
}
