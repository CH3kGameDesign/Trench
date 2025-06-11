using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Item/RevivePotion", fileName = "New Revive Potion")]
public class Item_RevivePotion : Item_Consumable
{
    public override bool Use(BaseController _cont)
    {
        if (!base.Use(_cont))
            return false;
        return true;
    }
}
