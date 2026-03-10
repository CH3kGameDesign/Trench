using UnityEngine;
using System.Collections.Generic;
using PurrNet;
using System;
using System.IO;

public static class SaveData
{
    public static int saveDataNum = 0;
    public static Themes.themeEnum themeCurrent = Themes.themeEnum.spaceStation;

    public static Mission missionCurrent = null;

    public static SaveClass Data = new SaveClass();
    [System.Serializable]
    public class SaveClass
    {
        //Currency
        public int i_currency;
        public List<Resource.resourceClass> resources;

        public List<GraffitiManager.graffitiClass> graffitiTags;
        public List<GraffitiManager.graffitiClass> graffitiArmor;
        public List<GraffitiManager.graffitiClass> graffitiShips;

        public List<Consumable.save> consumables;
        public List<Gun_Type> ownedGun;
        public List<Armor_Type> ownedArmor;

        public Layout_Defined.saveClass shipLayout = null;

        public Vector2Int i_equippedGunNum;
        public Gun_Type[] equippedGuns;
        public Armor_Type[] equippedArmor;

        public SaveClass()
        {
            i_currency = 200;
            resources = new List<Resource.resourceClass>();
            consumables = new List<Consumable.save>();
            graffitiTags = new List<GraffitiManager.graffitiClass>();
            graffitiArmor = new List<GraffitiManager.graffitiClass>();
            graffitiShips = new List<GraffitiManager.graffitiClass>();

            ownedGun = new List<Gun_Type>();
            ownedArmor = new List<Armor_Type>();

            if (SaveData.shipLayout != null)
            {
                if (SaveData.shipLayout._theme != null)
                    shipLayout = new Layout_Defined.saveClass(SaveData.shipLayout);
                else
                    shipLayout = null;
            }
            else
                shipLayout = null;

            i_equippedGunNum = new Vector2Int(0,1);
            equippedGuns = new Gun_Type[]
            {
                Gun_Type.gun_Rifle,
                Gun_Type.gun_Shotgun,
                Gun_Type.gun_Rod,
                Gun_Type.gun_Rocket
            };

            equippedArmor = new Armor_Type[]
            {
            Armor_Type.Helmet_Conscript,
            Armor_Type.Chest_Basic,
            Armor_Type.Arm_Basic,
            Armor_Type.Arm_Basic,
            Armor_Type.Leg_Basic,
            Armor_Type.Material_Black
            };
        }
    }
    public static Layout_Defined shipLayout = new Layout_Defined();

    public static List<Objective.objectiveClass> objectives = new List<Objective.objectiveClass>();

    public static settingsClass settings = new settingsClass();
    [System.Serializable]
    public class settingsClass
    {
        public int windowType = 0;

        public int sensitivityMouse = 50;
        public int sensitivityController = 50;

        public int audioMusic = 80;
        public int audioSFX = 80;
    }

    public static ArmorManager.SetClass equippedArmorSet = new ArmorManager.SetClass();

    public static Resource.resourceClass GetResource(Resource_Type _type)
    {
        foreach (var item in Data.resources)
        {
            if (item._type == _type)
                return item;
        }
        return null;
    }
    public static void Save() { Save(saveDataNum); }
    public static void Save(int _save)
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "SaveData")))
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "SaveData"));

        string path = Path.Combine(Application.persistentDataPath, "SaveData", "save_" + _save.ToString() + ".json");
        string json = JsonUtility.ToJson(Data);

        File.WriteAllText(path, json);
        Debug.Log("Game Saved to: " + path);
    }
    public static void Load() { Load(saveDataNum); }
    public static void Load(int _save)
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "SaveData")))
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "SaveData"));

        string path = Path.Combine(Application.persistentDataPath, "SaveData", "save_" + _save.ToString() + ".json");
        if (LoadFromFile(path, out string json))
        {
            SaveClass _temp = JsonUtility.FromJson<SaveClass>(json);
            Data = _temp;
            LoadFinal();
        }
    }

    static void LoadFinal()
    {
        //Data.ShipLayout doesn't seem to load
        if (Data.shipLayout != null)
            shipLayout = new Layout_Defined(Data.shipLayout);
    }
    
    public static bool LoadFromFile(string a_FileName, out string result)
    {
        var fullPath = Path.Combine(Application.persistentDataPath, a_FileName);

        try
        {
            result = File.ReadAllText(fullPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read from {fullPath} with exception {e}");
            result = "";
            return false;
        }
    }

    public static string lastLandingSpot = "tavern";

}
