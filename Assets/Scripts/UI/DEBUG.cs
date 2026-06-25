using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DEBUG : MonoBehaviour
{
    public GameObject G_cheatsActive;
    public TMP_InputField TMP_moneyAmt;
    public TMP_InputField TMP_dayAmt;
    static bool _isInvincible_Player = false;
    static bool _isInvincible_Companion = false;
    static bool _instakill = false;
    static bool _affordAnything = false;
    public static bool isInvincible_Player() { return _isInvincible_Player; } 
    public static bool isInvincible_Companion() { return _isInvincible_Companion; } 
    public static bool instakill() { return _instakill; } 
    public static bool affordAnything() { return _affordAnything; } 
    public static bool isCheating() { return _affordAnything || _isInvincible_Player || _isInvincible_Companion || _instakill; } 

    public void IsInvincible_Player (bool _value) { _isInvincible_Player = _value; UpdateCheatIndicator(); }
    public void isInvincible_Companion (bool _value) { _isInvincible_Companion = _value; UpdateCheatIndicator(); }
    public void Instakill (bool _value) { _instakill = _value; UpdateCheatIndicator(); }
    public void AffordAnything (bool _value) { _affordAnything = _value; UpdateCheatIndicator(); }
    public void AddMoney()
    {
        int _amt;
        if (int.TryParse(TMP_moneyAmt.text, out _amt))
            SaveData.Data.i_currency += _amt;
        MainMenu.Instance.main.UpdateCurrency(SaveData.Data.i_currency);
    }
    public void ClearMoney()
    {
        SaveData.Data.i_currency = 0;
        MainMenu.Instance.main.UpdateCurrency(SaveData.Data.i_currency);
    }

    public void SetDay()
    {
        int _amt = SaveData.Data.i_dayCounter;
        if (int.TryParse(TMP_dayAmt.text, out _amt))
        {
            if (_amt > 0)
            {
                _amt -= 1;
                _amt *= 2;
            }
            else
                _amt = SaveData.Data.i_dayCounter;
        }
        if (_amt % 2 == 1)
            _amt -= 1;
        SaveData.Data.i_dayCounter = _amt;
        Reload();
    }
    public void SetNight()
    {
        int _amt = SaveData.Data.i_dayCounter;
        if (int.TryParse(TMP_dayAmt.text, out _amt))
        {
            if (_amt > 0)
            {
                _amt -= 1;
                _amt *= 2;
            }
            else
                _amt = SaveData.Data.i_dayCounter;
        }
        if (_amt % 2 == 0)
            _amt += 1;
        SaveData.Data.i_dayCounter = _amt;
        Reload();
    }

    void Reload()
    {
        SaveData.Save();
        int _id = -1;
        if (SaveData.missionCurrent != null)
            _id = SaveData.missionCurrent._id;
        LevelGen_Holder.LoadTheme(SaveData.themeCurrent, _id);
    }
    void UpdateCheatIndicator()
    {
        G_cheatsActive.SetActive(isCheating());
    }
}
