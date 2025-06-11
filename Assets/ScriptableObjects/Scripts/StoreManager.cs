using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Trench/AssetLists/Store", fileName = "New Store")]
public class StoreManager : ScriptableObject
{
    public enum enumType { guns,  armor, consumable}
    public Sprite gunIcon;
    public List<GunClass> _guns = new List<GunClass>();
    [Space(10)]
    public Sprite armorIcon;
    public List<ArmorPiece> _armor = new List<ArmorPiece>();
    [Space(10)]
    public Sprite consumableIcon;
    public List<Item_Consumable> _consumable = new List<Item_Consumable>();
}
