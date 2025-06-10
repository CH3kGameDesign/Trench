using UnityEngine;
using System.Collections.Generic;

public static class SaveData
{
    public static Themes.themeEnum themeCurrent = Themes.themeEnum.spaceStation;

    public static int i_currency = 0;
    public static List<Objective.objectiveClass> objectives = new List<Objective.objectiveClass>();
    public static List<Resource.resourceClass> resources = new List<Resource.resourceClass>();
    public static List<Consumable.consumableClass> consumables = new List<Consumable.consumableClass>();

    public static List<Gun_Type> ownedGun = new List<Gun_Type>();
    public static List<Armor_Type> ownedArmor = new List<Armor_Type>();

    public static Armor_Type[] equippedArmor = new Armor_Type[]
    {
        Armor_Type.Helmet_Basic,
        Armor_Type.Chest_Basic,
        Armor_Type.Arm_Basic,
        Armor_Type.Arm_Basic,
        Armor_Type.Leg_Basic,
        Armor_Type.Material_Black
    };

    public static Resource.resourceClass GetResource(Resource_Type _type)
    {
        foreach (var item in resources)
        {
            if (item._type == _type)
                return item;
        }
        return null;
    }

    public static string lastLandingSpot = "tavern";
}
