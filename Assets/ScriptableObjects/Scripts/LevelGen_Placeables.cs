using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/LevelGen/Placeables", fileName = "New Placeables Manager")]
public class LevelGen_Placeables : ScriptableObject
{
    public static LevelGen_Placeables Instance;
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    public List<Furniture> furniture = new List<Furniture>();
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
    public class Furniture : PlaceableClass
    {
        public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null, string filePath = "Assets/Art/Sprites/LevelGen/")
        {
            filePath += "Furniture/";
            base.GenerateTexture(_camera, pos, rot, onEmpty, prefab, filePath);
        }
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
        foreach (var item in furniture)
            item.GenerateTexture(_camera, pos, rot);
        DestroyImmediate(_camera.gameObject);
        AssetDatabase.Refresh();
    }
#endif
}
