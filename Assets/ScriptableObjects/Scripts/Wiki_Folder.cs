using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wiki Folder", menuName = "Trench/Wiki/Folder")]
public class Wiki_Folder : ScriptableObject
{
    public pageSubClass landingPage;
    [System.Serializable]
    public class pageSubClass
    {
        public Wiki_Page page;
        public List<pageSubClass> children = new List<pageSubClass>();
    }
}
