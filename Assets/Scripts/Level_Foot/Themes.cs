using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Theme Holder", menuName = "Trench/LevelGen/Theme Holder")]
public class Themes : ScriptableObject
{
    public static Themes Instance = null;
    [Header("Themes")]
    public LevelGen_Theme theme_Default;
    public LevelGen_Theme theme_SpaceStation;
    public LevelGen_Theme theme_Ship;
    public List<LevelGen_Theme> theme_List = new List<LevelGen_Theme>();
    public List<Layout_Bounds> bounds_List = new List<Layout_Bounds>();
    public enum themeEnum { none = -1, _default, spaceStation, ship};
    public void Setup()
    {
        Instance = this;
    }

    public LevelGen_Theme GetTheme(themeEnum _theme)
    {
        switch (_theme)
        {
            case themeEnum._default: return theme_Default;
            case themeEnum.spaceStation: return theme_SpaceStation;
            case themeEnum.ship: return theme_Ship;
            default: return theme_Default;
        }
    }
    public LevelGen_Theme GetTheme(string _name)
    {
        foreach (var item in theme_List)
        {
            if (item.name == _name)
                return item;
        }
        return null;
    }
    public Layout_Bounds GetBounds (string _name)
    {
        foreach (var item in bounds_List)
        {
            if (item.name == _name)
                return item;
        }
        return null;
    }
}
