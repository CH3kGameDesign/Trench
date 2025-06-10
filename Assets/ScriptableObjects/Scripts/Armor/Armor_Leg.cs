using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Armor/Leg", fileName = "New Leg Armor")]
public class Armor_Leg : ArmorPiece
{
    public override enumType GetEnumType() { return enumType.armorLeg; }

    public GameObject modelKnee;
    public GameObject modelShin;
    public GameObject modelFoot;
    public override void Equip(RagdollManager _RM, bool _left = true)
    {
        _RM.T_armorPoints[7].DeleteChildren();
        _RM.T_armorPoints[8].DeleteChildren();
        _RM.T_armorPoints[9].DeleteChildren();
        _RM.T_armorPoints[10].DeleteChildren();
        _RM.T_armorPoints[11].DeleteChildren();
        _RM.T_armorPoints[12].DeleteChildren();
        if (modelKnee != null) Instantiate(modelKnee, _RM.T_armorPoints[7]);
        if (modelShin != null) Instantiate(modelShin, _RM.T_armorPoints[8]);
        if (modelFoot != null) Instantiate(modelFoot, _RM.T_armorPoints[9]);
        if (modelKnee != null) Instantiate(modelKnee, _RM.T_armorPoints[10]);
        if (modelShin != null) Instantiate(modelShin, _RM.T_armorPoints[11]);
        if (modelFoot != null) Instantiate(modelFoot, _RM.T_armorPoints[12]);
    }
    public override void AssignToPlayer(bool _left = true)
    {
        SaveData.equippedArmorSet.legs = this;
    }
    public override Transform[] Hooks(RagdollManager _RM, bool _left = true)
    {
        return new Transform[]
            {
                    _RM.T_armorPoints[7],
                    _RM.T_armorPoints[8],
                    _RM.T_armorPoints[9],
                    _RM.T_armorPoints[10],
                    _RM.T_armorPoints[11],
                    _RM.T_armorPoints[12]
            };
    }
    public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null,
        string filePath = "Assets/Art/Sprites/Armor/")
    {
        filePath += "Leg/";
        base.GenerateTexture(_camera, pos, rot, onEmpty, modelKnee, filePath);
    }
}
