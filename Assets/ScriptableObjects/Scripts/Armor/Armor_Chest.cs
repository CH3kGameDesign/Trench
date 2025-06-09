using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Armor/Chest", fileName = "New Chest Armor")]
public class Armor_Chest : ArmorPiece
{
    public override enumType GetEnumType() { return enumType.armorChest; }

    public GameObject modelChest;
    public GameObject modelBelt;
    public override void Equip(RagdollManager _RM, bool _left = true)
    {
        _RM.T_armorPoints[1].DeleteChildren();
        _RM.T_armorPoints[2].DeleteChildren();
        if (modelChest != null) Instantiate(modelChest, _RM.T_armorPoints[1]);
        if (modelBelt != null) Instantiate(modelBelt, _RM.T_armorPoints[2]);
    }
    public override Transform[] Hooks(RagdollManager _RM, bool _left = true)
    {
        return new Transform[]
            {
                    _RM.T_armorPoints[2]
            };
    }
    public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null,
        string filePath = "Assets/Art/Sprites/Armor/")
    {
        pos = new Vector3(0, -1.33f, 3.3f);
        rot = new Vector3(0, 180, 0);
        filePath += "Chest/";
        base.GenerateTexture(_camera, pos, rot, onEmpty, modelChest, filePath);
    }
}
