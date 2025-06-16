using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGen_Holder : MonoBehaviour
{
    public static LevelGen_Holder Instance;
    public List<LevelGen> List = new List<LevelGen>();

    public int I_maxDecals = 100;
    private List<Decal_Handler> d_Handlers = new List<Decal_Handler>();

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

    public void AddDecal(Decal_Handler handler)
    {
        d_Handlers.Add(handler);
        if (d_Handlers.Count > I_maxDecals)
        {
            d_Handlers[0].Remove();
            d_Handlers.RemoveAt(0);
        }
    }

    public void RemoveDecal(Decal_Handler handler)
    {
        if (d_Handlers.Contains(handler))
            d_Handlers.Remove(handler);
    }
}
