using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Basic Layout", menuName = "Trench/Layout/Basic")]
public class Layout_Basic : ScriptableObject
{
    public List<blockColumn> data = new List<blockColumn>();


    [System.Serializable]
    public class block
    {
        public blockType type;
        public roomEnum roomtype = roomEnum.empty;
        public entryTypeEnum entryType = entryTypeEnum.empty;
        public block()
        {
            this.type = blockType.empty;
            this.roomtype = roomEnum.empty;
            this.entryType = entryTypeEnum.empty;
        }
        public block(blockType _type)
        {
            this.type = _type;
            this.roomtype = roomEnum.empty;
            this.entryType = entryTypeEnum.empty;
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
    public enum blockType { room, entry, empty }
    public enum roomEnum {
        [InspectorName("Empty")] empty,

        [InspectorName("Room/Bridge")] bridge,
        [InspectorName("Room/Hangar")] hangar,
        [InspectorName("Room/Corridor")] corridor,

        [InspectorName("Add/Above")] addAbove,
        [InspectorName("Add/Below")] addBelow,
        [InspectorName("Add/Left")] addLeft,
        [InspectorName("Add/Right")] addRight,
        [InspectorName("Remove/Row")] removeRow,
        [InspectorName("Remove/Column")] removeColumn
    };
    public enum entryTypeEnum {
        [InspectorName("Empty")] empty,

        [InspectorName("Door/Single")] singleDoor,
        [InspectorName("Door/Wide")] wideDoor,

        [InspectorName("Vent")] vent,

        [InspectorName("")] shipDoor,
        [InspectorName("")] shipPark,

        [InspectorName("Any")] any
    }
}
