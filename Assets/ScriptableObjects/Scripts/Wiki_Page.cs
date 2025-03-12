using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu (fileName = "New Wiki Page", menuName = "Trench/Wiki/Page")]
public class Wiki_Page : ScriptableObject
{
    public List<section> data = new List<section>();
    public enum sectionEnum { O__FORMAT__O, basic, twoSplit, threeSplit, O__FUNCTION__O, moveUp, moveDown, remove};
    public enum splitEnum { text, image };
    [System.Serializable]
    public class section
    {
        public string id;
        public sectionEnum type = sectionEnum.basic;
        public List<split> splits;

        public section()
        {
            this.id = "New";
            this.type = sectionEnum.basic;
            this.splits = new List<split>() { new split(), new split(), new split() };
        }
        public section(sectionEnum type, List<split> splits)
        {
            this.id = "New";
            this.type = type;
            this.splits = splits;
        }
    }
    [System.Serializable]
    public class split
    {
        public splitEnum splitType;

        [ContextMenuItem("Switch To Image", "SwitchToImage")]
        public string text;
        [ContextMenuItem("Switch To Text", "SwitchToText")]
        public Sprite image;

        [ContextMenu("Switch To Text")]
        public void SwitchToText() { splitType = splitEnum.text; }
        [ContextMenu("Switch To Image")]
        public void SwitchToImage() { splitType = splitEnum.image; }

        public split()
        {
            this.splitType = splitEnum.text;
            this.text = "";
            this.image = null;
        }
    }
}
