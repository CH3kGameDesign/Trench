using UnityEngine;
using System.Collections.Generic;
using PurrNet;

public static class SaveData
{
    public static Themes.themeEnum themeCurrent = Themes.themeEnum.spaceStation;

    public static int i_currency = 200;
    public static List<Objective.objectiveClass> objectives = new List<Objective.objectiveClass>();
    public static List<Resource.resourceClass> resources = new List<Resource.resourceClass>();
    public static List<Consumable.save> consumables = new List<Consumable.save>();

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

    public static Vector2Int i_equippedGunNum = new Vector2Int(0,1);
    public static Gun_Type[] equippedGuns = new Gun_Type[]
    {
        Gun_Type.gun_Rifle,
        Gun_Type.gun_Shotgun,
        Gun_Type.gun_Rod
    };

    public static ArmorManager.SetClass equippedArmorSet = new ArmorManager.SetClass();

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
