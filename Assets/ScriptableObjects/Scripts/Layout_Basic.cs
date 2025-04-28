using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Basic Layout", menuName = "Trench/Layout/Basic")]
public class Layout_Basic : ScriptableObject
{
    public List<blockColumn> data = new List<blockColumn>();
    public room recipe = new room();

    public Vector2Int extraRoom_Amount = Vector2Int.zero;

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
    public enum roomEnum
    {
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
    public enum entryTypeEnum
    {
        [InspectorName("Empty")] empty,

        [InspectorName("Door/Single")] singleDoor,
        [InspectorName("Door/Wide")] wideDoor,

        [InspectorName("Vent")] vent,

        [InspectorName("")] shipDoor,
        [InspectorName("")] shipPark,

        [InspectorName("Any")] any
    }

    [System.Serializable]
    public class room
    {
        public roomEnum roomType;

        public List<room> connectedRooms = new List<room>();
        public List<entryTypeEnum> entryTypes = new List<entryTypeEnum>();
    }
    private List<Vector2Int> checkedRooms = new List<Vector2Int>();
    public void UpdateRecipe()
    {
        Vector2Int firstRoom = GetFirstRoom();
        if (firstRoom.x >= 0)
        {
            checkedRooms = new List<Vector2Int>();
            AddRoom(firstRoom);
        }
    }
    public void AddRoom(Vector2Int _gridPos, room _parent = null, entryTypeEnum _entry = entryTypeEnum.any)
    {
        if (!checkedRooms.Contains(_gridPos) && _entry != entryTypeEnum.empty)
        {
            if (_gridPos.y >= 0 && _gridPos.y < data.Count &&
                _gridPos.x >= 0 && _gridPos.x < data[0].d.Count)
            {
                block _block = data[_gridPos.y].d[_gridPos.x];
                if (_block.roomtype != roomEnum.empty)
                {
                    checkedRooms.Add(_gridPos);
                    room _room = new room();
                    _room.roomType = _block.roomtype;
                    if (_parent == null)
                        recipe = _room;
                    else
                    {
                        _parent.connectedRooms.Add(_room);
                        _parent.entryTypes.Add(_entry);
                    }

                    if (_gridPos.y > 1)
                        AddRoom(new Vector2Int(_gridPos.x, _gridPos.y - 2), _room,
                            data[_gridPos.y - 1].d[_gridPos.x].entryType);
                    if (_gridPos.y < data.Count - 2)
                        AddRoom(new Vector2Int(_gridPos.x, _gridPos.y + 2), _room,
                            data[_gridPos.y + 1].d[_gridPos.x].entryType);

                    if (_gridPos.x > 1)
                        AddRoom(new Vector2Int(_gridPos.x - 2, _gridPos.y), _room,
                            data[_gridPos.y].d[_gridPos.x - 1].entryType);
                    if (_gridPos.x < data[0].d.Count - 2)
                        AddRoom(new Vector2Int(_gridPos.x + 2, _gridPos.y), _room,
                            data[_gridPos.y].d[_gridPos.x + 1].entryType);
                }
            }
        }
    }

    public Vector2Int GetFirstRoom()
    {
        for (int y = 0; y < data.Count; y += 2)
        {
            for (int x = 0; x < data[y].d.Count; x += 2)
            {
                if (data[y].d[x].roomtype != roomEnum.empty)
                    return new Vector2Int(x, y);
            }
        }
        Debug.LogError("Couldn't Find Non-Empty Room");
        return new Vector2Int(-1, -1);
    }
}
