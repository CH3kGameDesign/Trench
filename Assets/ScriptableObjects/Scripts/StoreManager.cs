using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Trench/AssetLists/Store", fileName = "New Store")]
public class StoreManager : ScriptableObject
{
    public enum enumType { guns,  armor}
    public Sprite gunIcon;
    public List<GunClass> _guns = new List<GunClass>();
    [Space(10)]
    public Sprite armorIcon;
    public List<ArmorPiece> _armor = new List<ArmorPiece>();
}
