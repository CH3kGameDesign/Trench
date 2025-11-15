using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Stamp", fileName = "New Stamp")]
public class Stamp_Scriptable : ScriptableObject
{
    public GraffitiManager.stampTypeEnum stampType;
    public string _name = "";
    public Sprite _sprite;
}
