using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGen_Holder : MonoBehaviour
{
    public static LevelGen_Holder Instance;
    public List<LevelGen> List = new List<LevelGen>();
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public int GetCollectedValue()
    {
        int _value = 0;
        foreach (var LG in List)
        {
            _value += LG.GetCollectedValue();
        }
        return _value;
    }

    public static void LoadTheme(Themes.themeEnum _theme)
    {
        SaveData.themeCurrent = _theme;
        SaveData.i_currency += Instance.GetCollectedValue();
        SceneManager.LoadScene(0);
    }
}
