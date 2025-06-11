using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Armor/Material", fileName = "New Material Armor")]
public class Armor_Material : ArmorPiece
{
    public override enumType GetEnumType() { return enumType.armorMat; }

    public Material modelMaterial;
    public override void Equip(RagdollManager _RM, bool _left = true)
    {
        _RM.MR_skinnedMeshRenderer.material = modelMaterial;
    }
    public override void AssignToPlayer(bool _left = true)
    {
        SaveData.equippedArmorSet.material = this;
    }
    public override Transform[] Hooks(RagdollManager _RM, bool _left = true)
    {
        return new Transform[]
            {
                    _RM.T_armorPoints[0],
                    _RM.T_armorPoints[2]
            };
    }

    public override void GenerateTexture(Camera _camera, Vector3 pos, Vector3 rot, Texture2D onEmpty = null, GameObject _model = null,
        string filePath = "Assets/Art/Sprites/Armor/")
    {
        if (_model != null)
        {
            GameObject GO;
            filePath += "Material/";
            string targetPath = filePath + name + ".png";
            GO = Instantiate(_model);
            GO.GetComponent<MeshRenderer>().material = modelMaterial;
            pos = Vector3.forward * 6;
            GO.transform.position = pos;
            _camera.Render();
#if UNITY_EDITOR
            _camera.activeTexture.SaveToFile(targetPath, SetImage);
#endif
            DestroyImmediate(GO);
        }
    }
}
