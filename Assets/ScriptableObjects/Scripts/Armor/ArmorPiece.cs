using UnityEngine;

public class ArmorPiece : ItemClass
{
    public virtual void Equip(RagdollManager _RM, bool _left = true)
    {

    }
    public virtual void AssignToPlayer(bool _left = true)
    {

    }
    public virtual Armor_Type GetEnum()
    {
        string _string = _id.Replace('/', '_');
        if (Armor_Type.TryParse(_string, out Armor_Type _temp))
            return _temp;
        Debug.LogError("Can't find Armor_Type Enum:" + _string);
        return Armor_Type.Helmet_Empty;
    }
    public override void Setup()
    {
        Armor_Type _enum = GetEnum();
        if (SaveData.ownedArmor.Contains(_enum))
            ownedAmt = 1;
        else
        {
            if (cost.unlockAtStart)
            {
                ownedAmt = 1;
                SaveData.ownedArmor.Add(_enum);
            }
            else
                ownedAmt = 0;
        }
    }
    public virtual Transform[] Hooks(RagdollManager _RM, bool _left = true)
    {
        return new Transform[0];
    }
    public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null,
        string filePath = "Assets/Art/Sprites/Armor/")
    {
        base.GenerateTexture(_camera, pos, rot, onEmpty, _model, filePath);
    }
    public override void Purchase()
    {
        base.Purchase();
        Armor_Type _enum = GetEnum();
        if (!SaveData.ownedArmor.Contains(_enum))
            SaveData.ownedArmor.Add(_enum);
        PlayerManager.main.DebugGunList();
        PlayerManager.main.Setup_Radial();
    }
}
