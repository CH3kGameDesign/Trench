using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Armor/Arm", fileName = "New Arm Armor")]
public class Armor_Arm : ArmorPiece
{
    public override enumType GetEnumType() { return enumType.armorArm; }

    public GameObject modelArm;
    public GameObject modelWrist;
    public override void Equip(RagdollManager _RM, bool _left = true)
    {
        if (_left)
        {
            _RM.T_armorPoints[3].DeleteChildren();
            _RM.T_armorPoints[4].DeleteChildren();
            if (modelArm != null) Instantiate(modelArm, _RM.T_armorPoints[3]);
            if (modelWrist != null) Instantiate(modelWrist, _RM.T_armorPoints[4]);
        }
        else
        {
            _RM.T_armorPoints[5].DeleteChildren();
            _RM.T_armorPoints[6].DeleteChildren();
            if (modelArm != null) Instantiate(modelArm, _RM.T_armorPoints[5]);
            if (modelWrist != null) Instantiate(modelWrist, _RM.T_armorPoints[6]);
        }
    }
    public override void AssignToPlayer(bool _left = true)
    {
        if (_left)
            SaveData.equippedArmorSet.armL = this;
        else
            SaveData.equippedArmorSet.armR = this;
    }
    public override Transform[] Hooks(RagdollManager _RM, bool _left = true)
    {
        if (_left)
            return new Transform[]
                {
                        _RM.T_armorPoints[3],
                        _RM.T_armorPoints[4]
                };
        else
            return new Transform[]
                {
                        _RM.T_armorPoints[5],
                        _RM.T_armorPoints[6]
                };
    }
    public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null,
        string filePath = "Assets/Art/Sprites/Armor/")
    {
        filePath += "Arm/";
        base.GenerateTexture(_camera, pos, rot, onEmpty, modelArm, filePath);
    }
}
