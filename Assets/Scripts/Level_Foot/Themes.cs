using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Theme Holder", menuName = "Trench/LevelGen/Theme Holder")]
public class Themes : ScriptableObject
{
    [Header("Themes")]
    public LevelGen_Theme theme_Default;
    public LevelGen_Theme theme_SpaceStation;
    public LevelGen_Theme theme_Ship;
    public enum themeEnum { none = -1, _default, spaceStation, ship};


    public LevelGen_Theme GetTheme (themeEnum _theme)
    {
        switch (_theme)
        {
            case themeEnum._default:        return theme_Default;
            case themeEnum.spaceStation:    return theme_SpaceStation;
            case themeEnum.ship:            return theme_Ship;
            default:                        return theme_Default;
        }
    }
}
