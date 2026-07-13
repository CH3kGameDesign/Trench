using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Trench/AssetLists/Stamp List", fileName = "New Stamp List")]
public class Stamp_List : ScriptableObject
{
    public List<Stamp_Scriptable> list  = new List<Stamp_Scriptable>();

    private string stampDirectory = "/ScriptableObjects/Stamps/";
    private string primitiveSubDirectory = "Primitives/";
    private string textSubDirectory = "Text/";
    private string complexSubDirectory = "Complex/";


#if UNITY_EDITOR
    [ContextMenu("Collect Stamps")]
    public void CollectStamps()
    {
        CollectStamps(GraffitiManager.stampTypeEnum.primitives);
        CollectStamps(GraffitiManager.stampTypeEnum.letters);
        CollectStamps(GraffitiManager.stampTypeEnum.complex);
    }
    void CollectStamps(GraffitiManager.stampTypeEnum stampType)
    {
        string _location = stampDirectory;
        switch (stampType)
        {
            case GraffitiManager.stampTypeEnum.primitives:
                _location += primitiveSubDirectory;
                break;
            case GraffitiManager.stampTypeEnum.letters:
                _location += textSubDirectory;
                break;
            case GraffitiManager.stampTypeEnum.complex:
                _location += complexSubDirectory;
                break;
            default:
                break;
        }

        string mainPath = Application.dataPath + _location;
        string[] filePaths = Directory.GetFiles(mainPath, "*.asset",
                                         SearchOption.AllDirectories);
        foreach (var path in filePaths)
        {
            string _path = "Assets" + path.Substring(Application.dataPath.Length);
            Stamp_Scriptable _temp = (Stamp_Scriptable)AssetDatabase.LoadAssetAtPath(_path, typeof(Stamp_Scriptable));
            if (_temp != null)
            {
                _temp.stampType = stampType;
                if (_temp._stampID == 0)
                    _temp._stampID = NewStampID();
                if (!list.Contains(_temp))
                    list.Add(_temp);
                EditorUtility.SetDirty(_temp);
            }
        }

        EditorUtility.SetDirty(this);
    }

    int NewStampID()
    {
        bool _valid = false;
        int _stampID = 0;
        while (_valid == false)
        {
            _valid = true;
            _stampID = Random.Range(1, int.MaxValue);
            foreach (var item in list)
            {
                if (_stampID == item._stampID)
                {
                    _valid = false;
                    break;
                }
            }
        }
        return _stampID;
    }
#endif
}