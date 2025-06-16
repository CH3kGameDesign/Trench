using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class Prefab_Save : MonoBehaviour
{
    private string S_root = "Assets/Prefabs/Environment";
    public string S_themeName = "Default";
    public List<GameObject> G_holders = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
#if UNITY_EDITOR
    public void SavePrefabs()
    {
        string root;
        string folder;
        string filePath;

        string[] initialPath = S_root.Split('/');
        root = initialPath[0];
        for (int i = 1; i < initialPath.Length; i++)
        {
            if (!Directory.Exists(root + "/" + initialPath[i]))
            {
                AssetDatabase.CreateFolder(root, initialPath[i]);
                Debug.Log(initialPath[i]);
            }
            root += "/" + initialPath[i];
        }
        if (!Directory.Exists(S_root + "/" + S_themeName))
        {
            AssetDatabase.CreateFolder(S_root, S_themeName);
            Debug.Log(S_root + "/" + S_themeName);
        }
        foreach (var holder in G_holders)
        {
            folder = holder.name.Trim('-');
            root = S_root + "/" + S_themeName + "/" + folder;
            if (!Directory.Exists(root))
            {
                AssetDatabase.CreateFolder(S_root + "/" + S_themeName, folder);
                Debug.Log(root);
            }
            for (int i = 0; i < holder.transform.childCount; i++)
            {
                GameObject _object = holder.transform.GetChild(i).gameObject;
                filePath = root + "/" + _object.name + ".prefab";
                bool prefabSuccess;
                GameObject GO = PrefabUtility.SaveAsPrefabAsset(_object, filePath, out prefabSuccess);
                GO.transform.localPosition = Vector3.zero;
                GO.transform.localEulerAngles = Vector3.zero;
                PrefabUtility.SavePrefabAsset(GO);
                if (prefabSuccess == true)
                    Debug.Log("Prefab was saved successfully");
                else
                    Debug.Log("Prefab failed to save" + prefabSuccess);
            }
        }
    }
#endif
}
