using System;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ItemClass : ScriptableObject
{
    public virtual enumType GetEnumType() { return enumType.undefined; }

    public string _id;
    public string _name;
    public Texture2D image;

    public int sortOrder = 0;
    public enum enumType {
        gun, 
        armorHead, armorChest, armorArm, armorLeg, armorMat,
        consumable, 
        upgrade, 
        undefined
    }
    public virtual void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null,
        string filePath = "Assets/Art/Sprites/")
    {
        if (_model != null)
        {
            GameObject GO;
            string targetPath = filePath + name.Replace("/", "_") + ".png";
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
