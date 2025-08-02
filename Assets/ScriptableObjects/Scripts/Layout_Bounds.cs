using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Layout Bounds", menuName = "Trench/Layout/Bounds")]
public class Layout_Bounds : ScriptableObject
{
    public List<blockColumn> data = new List<blockColumn>();

    public List<List<int>> list = new List<List<int>>();

    [System.Serializable]
    public class block
    {
        public roomEnum roomtype = roomEnum.empty;
        public block()
        {
            this.roomtype = roomEnum.empty;
        }
    }

    [System.Serializable]
    public class blockColumn
    {
        public List<block> d = new List<block>();
        public blockColumn()
        {
            this.d = new List<block>();
        }
    }
    public enum roomEnum
    {
        [InspectorName("Empty")] empty = -1,

        [InspectorName("Placeable")] placeable = 0,

        [InspectorName("Add/Above")] addAbove = 100,
        [InspectorName("Add/Below")] addBelow = 101,
        [InspectorName("Add/Left")] addLeft = 102,
        [InspectorName("Add/Right")] addRight = 103,
        [InspectorName("Remove/Row")] removeRow = 104,
        [InspectorName("Remove/Column")] removeColumn = 105
    }

}
