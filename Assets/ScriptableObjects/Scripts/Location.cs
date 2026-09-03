using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Location", fileName = "New Location")]
public class Location : ScriptableObject
{
    public string locationID = "";
    [Space(10)]
    public string S_name;
    public Texture2D T2_icon;
    [Space(10)]
    public Themes.themeEnum theme = Themes.themeEnum._default;
    public List<Mission> availableMissions = new List<Mission>();

    public Mission GetRandomMission()
    {
        if (availableMissions.Count == 0)
            return null;
        int _temp = Random.Range(0, availableMissions.Count);
        return availableMissions[_temp];
    }
}
