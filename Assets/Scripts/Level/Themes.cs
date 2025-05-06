using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Theme Holder", menuName = "Trench/LevelGen/Theme Holder")]
public class Themes : ScriptableObject
{
    [Header("Themes")]
    public LevelGen_Theme theme_Default;
    public LevelGen_Theme theme_SpaceStation;
    public enum themeEnum { _default, spaceStation};


    public LevelGen_Theme GetTheme (themeEnum _theme)
    {
        switch (_theme)
        {
            case themeEnum._default:        return theme_Default;
            case themeEnum.spaceStation:    return theme_SpaceStation;
            default:                        return theme_Default;
        }
    }
}
