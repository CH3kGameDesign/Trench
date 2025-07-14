using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Armor/Helmet", fileName = "New Helmet Armor")]
public class Armor_Helmet : ArmorPiece
{
    public override enumType GetEnumType() { return enumType.armorHead; }
    public GameObject modelHelmet;
    public override void Equip(RagdollManager _RM, bool _left = true)
    {
        _RM.T_armorPoints[0].DeleteChildren();
        if (modelHelmet != null) Instantiate(modelHelmet, _RM.T_armorPoints[0]);
        if (_RM.controller != null)
        {
            _RM.controller.SetIcon(image);
            _RM.controller.info.icon.value = GetEnum();
        }
    }
    public override void AssignToPlayer(bool _left = true)
    {
        SaveData.equippedArmorSet.helmet = this;
    }
    public override Transform[] Hooks(RagdollManager _RM, bool _left = true)
    {
        return new Transform[]
            {
                    _RM.T_armorPoints[0]
            };
    }
    public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null,
        string filePath = "Assets/Art/Sprites/Armor/")
    {
        filePath += "Helmet/";
        pos = new Vector3(0, -0.16f, 2);
        rot = new Vector3(-90, 90, 0);
        base.GenerateTexture(_camera, pos, rot, onEmpty, modelHelmet, filePath);
    }
}
