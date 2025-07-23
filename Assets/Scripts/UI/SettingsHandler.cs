using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Globalization;
using UnityEditor;

public class SettingsHandler : MonoBehaviour
{
    [System.Serializable]
    public class sliderOption
    {
        public Slider S_slider;
        public TextMeshProUGUI TM_text;

        public void Setup(int value)
        {
            S_slider.value = value;
            TM_text.text = value.ToString();
        }
        public void UpdateText()
        {
            TM_text.text = S_slider.value.ToString();
        }
    }
    public TMP_Dropdown windowDropdown;

    public sliderOption sensitivityMouse;
    public sliderOption sensitivityController;

    public sliderOption audioMusic;
    public sliderOption audioSFX;

    public AudioMixer masterMixer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Load();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    string wind = "windowType";
    string senM = "sensitivityMouse";
    string senC = "sensitivityController";
    string audM = "audioMouse";
    string audS = "audioSFX";
    private void Load()
    {
        if (PlayerPrefs.HasKey(wind)) SaveData.settings.windowType = PlayerPrefs.GetInt(wind);

        if (PlayerPrefs.HasKey(senM)) SaveData.settings.sensitivityMouse = PlayerPrefs.GetInt(senM);
        if (PlayerPrefs.HasKey(senC)) SaveData.settings.sensitivityController = PlayerPrefs.GetInt(senC);
        if (PlayerPrefs.HasKey(audM)) SaveData.settings.audioMusic = PlayerPrefs.GetInt(audM);
        if (PlayerPrefs.HasKey(audS)) SaveData.settings.audioSFX = PlayerPrefs.GetInt(audS);

        windowDropdown.value = SaveData.settings.windowType;

        sensitivityMouse.Setup(SaveData.settings.sensitivityMouse);
        sensitivityController.Setup(SaveData.settings.sensitivityController);
        audioMusic.Setup(SaveData.settings.audioMusic);
        audioSFX.Setup(SaveData.settings.audioSFX);

        masterMixer.SetFloat("musicVolume", Mathf.Log10(Mathf.Max(SaveData.settings.audioMusic / 80f, 0.0001f)) * 40f);
        masterMixer.SetFloat("sfxVolume", Mathf.Log10(Mathf.Max(SaveData.settings.audioSFX / 80f, 0.0001f)) * 40f);

        WindowSet(SaveData.settings.windowType);
    }

    public void UpdateDropdown_Window(Int32 _windowType)
    {
        PlayerPrefs.SetInt(wind, _windowType);
        SaveData.settings.windowType = _windowType;

        WindowSet(_windowType);
    }

    void WindowSet(int _windowType)
    {
        switch (_windowType)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            default:
                break;
        }
    }

    public void UpdateSlider_SensM(Single _value)
    {
        sensitivityMouse.UpdateText();
        PlayerPrefs.SetInt(senM, (int)_value);
        SaveData.settings.sensitivityMouse = (int)_value;

        if (PlayerManager.main)
            PlayerManager.main.UpdateSensitivity();
    }
    public void UpdateSlider_SensC(Single _value)
    {
        sensitivityController.UpdateText();
        PlayerPrefs.SetInt(senC, (int)_value);
        SaveData.settings.sensitivityController = (int)_value;

        if (PlayerManager.main)
            PlayerManager.main.UpdateSensitivity();
    }
    public void UpdateSlider_AudM(Single _value)
    {
        audioMusic.UpdateText();
        SaveData.settings.audioMusic = (int)_value;
        PlayerPrefs.SetInt(audM, (int)_value);
        masterMixer.SetFloat("musicVolume", Mathf.Log10(Mathf.Max(_value/80f,0.0001f)) * 40f);
    }
    public void UpdateSlider_AudS(Single _value)
    {
        audioSFX.UpdateText();
        SaveData.settings.audioSFX = (int)_value;
        PlayerPrefs.SetInt(audS, (int)_value);
        masterMixer.SetFloat("sfxVolume", Mathf.Log10(Mathf.Max(_value / 80f, 0.0001f)) * 40f);
    }
}
