using UnityEngine;

public class ArmorPiece : ItemClass
{
    public virtual void Equip(RagdollManager _RM, bool _left = true)
    {

    }
    public virtual Armor_Type _enum()
    {
        string _string = _id.Replace('/', '_');
        if (Armor_Type.TryParse(_string, out Armor_Type _temp))
            return _temp;
        Debug.LogError("Can't find Armor_Type Enum:" + _string);
        return Armor_Type.Helmet_Empty;
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
}
