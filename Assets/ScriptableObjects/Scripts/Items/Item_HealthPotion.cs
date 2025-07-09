using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Item/HealthPotion", fileName = "New Health Potion")]
public class Item_HealthPotion : Item_Consumable
{
    public override bool Use(BaseController _cont)
    {
        if (_cont.info.F_curHealth >= _cont.info.F_maxHealth)
            return false;
        if (!base.Use(_cont))
            return false;

        _cont.OnHeal(_cont.info.F_maxHealth);

        return true;
    }
}
