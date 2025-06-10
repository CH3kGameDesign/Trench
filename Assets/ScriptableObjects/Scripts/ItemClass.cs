using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;

public class ItemClass : ScriptableObject
{
    public virtual enumType GetEnumType() { return enumType.undefined; }

    public string _id;
    public string _name;
    public Texture2D image;
    public string _description;
    [HideInInspector]public int ownedAmt = 0;
    public enumRarity rarity = enumRarity.Common;

    public costClass cost;

    [System.Serializable]
    public class costClass
    {
        public bool unlockAtStart = false;
        public int coinCost = 0;
        public List<Resource.resourceClass> list = new List<Resource.resourceClass>();
    }

    public int sortOrder = 0;
    public enum enumType {
        gun, 
        armorHead, armorChest, armorArm, armorLeg, armorMat,
        consumable, 
        upgrade, 
        undefined
    }
    public enum enumRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
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

    public void OnClick()
    {
        MainMenu.Instance.store.UpdateItem(this);
    }
    public virtual void Setup()
    {
        if (cost.unlockAtStart)
            ownedAmt = 1;
        else
            ownedAmt = 0;
    }
    public virtual void Purchase()
    {
        ownedAmt++;
        SaveData.i_currency -= cost.coinCost;
        foreach (var item in cost.list)
        {
            Resource.resourceClass _temp = SaveData.GetResource(item._type);
            if (_temp != null)
                _temp.amt -= item.amt;
        }
    }
}
