using System.Collections.Generic;
using UnityEngine;

public class Item_Consumable : ItemClass
{
    public override enumType GetEnumType() { return enumType.consumable; }

    public string primary;
    public GameObject model;
    public Sprite sprite;
    public enum itemEnum { radial, contextual}
    public itemEnum itemType;
    public virtual bool Use(BaseController _cont)
    {
        Consumable.save _save = GetSave();
        if (_save == null)
            return false;
        if (_save._amt <= 0)
            return false;
        _save._amt--;
        return true;
    }

    public virtual Consumable_Type GetEnum()
    {
        string _string = _id.Replace('/', '_');
        if (Consumable_Type.TryParse(_string, out Consumable_Type _temp))
            return _temp;
        Debug.LogError("Can't find Consumable_Type Enum:" + _string);
        return Consumable_Type.Item_HealthPotion;
    }

    public override void Setup()
    {
        base.Setup();
    }
    public Consumable.save GetSave()
    {
        foreach (var item in SaveData.consumables)
        {
            if (item._type == GetEnum())
                return item;
        }
        Consumable.save _temp = Consumable.save.Create(GetEnum(),0);
        SaveData.consumables.Add(_temp);
        return _temp;
    }

    public Item_Consumable Clone(Consumable _consumable = null)
    {
        Item_Consumable _temp = new Item_Consumable();
        _temp._id = _id;
        _temp._name = _name;
        _temp.image = image;
        _temp.primary = primary;
        _temp._description = _description;
        _temp.ownedAmt = ownedAmt;
        _temp.rarity = rarity;
        _temp.cost = cost;
        _temp.sortOrder = sortOrder;

        _temp.itemType = itemType;
        return _temp;
    }
    public override void Purchase()
    {
        base.Purchase();
        Consumable.save _temp = GetSave();
        _temp._amt++;
        _temp._totalAmt++;
    }
}
