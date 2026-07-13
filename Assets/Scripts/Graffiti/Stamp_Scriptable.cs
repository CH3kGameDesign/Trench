using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Stamp", fileName = "New Stamp")]
public class Stamp_Scriptable : ScriptableObject
{
    public GraffitiManager.stampTypeEnum stampType;
    public string _name = "";
    public int _stampID = 0;
    public Sprite _sprite;
}
