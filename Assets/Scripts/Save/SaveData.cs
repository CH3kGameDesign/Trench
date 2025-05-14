using UnityEngine;
using System.Collections.Generic;

public static class SaveData
{
    public static Themes.themeEnum themeCurrent = Themes.themeEnum.spaceStation;
    public static List<Objective.objectiveClass> objectives = new List<Objective.objectiveClass>();
    public static List<Resource.resourceClass> resources = new List<Resource.resourceClass>();
    public static List<Consumable.consumableClass> consumables = new List<Consumable.consumableClass>();
}
