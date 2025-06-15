using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Item/RevivePotion", fileName = "New Revive Potion")]
public class Item_RevivePotion : Item_Consumable
{
    public override bool Use(BaseController _cont)
    {
        bool b = false;
        foreach (var item in _cont.followers)
        {
            if (item.F_curHealth <= 0)
            {
                b = true;
                break;
            }
        }
        if (!b)
            return false;
        if (!base.Use(_cont))
            return false;
        foreach (var item in _cont.followers)
        {
            if (item.F_curHealth <= 0)
            {
                if (item == PlayerController.Instance.BC_equippedController)
                    PlayerController.Instance.DropController();
                item.Revive();
            }
        }
        return true;
    }
}
