using UnityEngine;
using System.Collections.Generic;

public static class SaveData
{
    public static Themes.themeEnum themeCurrent = Themes.themeEnum.spaceStation;
    public static List<Objective.objectiveClass> objectives = new List<Objective.objectiveClass>();
    public static List<Resource.resourceClass> resources = new List<Resource.resourceClass>();
    public static List<Consumable.consumableClass> consumables = new List<Consumable.consumableClass>();

    public static Armor_Type[] equippedArmor = new Armor_Type[]
    {
        Armor_Type.Helmet_Basic,
        Armor_Type.Chest_Basic,
        Armor_Type.Arm_Basic,
        Armor_Type.Arm_Basic,
        Armor_Type.Leg_Basic
    };

    public static string lastLandingSpot = "tavern";
}
