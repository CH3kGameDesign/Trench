using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;
    public panelRefClass main;
    public customizeRefClass customize;
    public panelRefClass load;

    private panelRefClass _current = null;

    [System.Serializable]
    public class panelRefClass
    {
        public Animator _anim;
        public Button _defaultButton;
        public Button _backButton;

        public virtual GameObject DefaultButton(bool _back = false)
        {
            if (_back)
                return _backButton.gameObject;
            return _defaultButton.gameObject;
        }
    }

    [System.Serializable]
    public class customizeRefClass : panelRefClass
    {
        [HideInInspector] public int I_armorType = 0;
        public GameObject G_listOptions;
        public GameObject G_subOptions;

        public override GameObject DefaultButton(bool _back = false)
        {
            if (_back)
                return _backButton.gameObject;
            if (G_listOptions.activeSelf || G_subOptions.transform.childCount == 0)
                return _defaultButton.gameObject;
            else
                return G_subOptions.transform.GetChild(0).gameObject;
        }
    }
    public void SwitchTo(panelRefClass GO)
    {
        _current = GO;
        main._anim.SetBool("Open", main == GO);
        customize._anim.SetBool("Open", customize == GO);
        load._anim.SetBool("Open", load == GO);

        GamepadSwitch();
    }

    private bool menuOpen = false;
    public Canvas C_canvas;


    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Menu_Tapped()
    {
        if (menuOpen) Close();
        else Open();
    }

    private BaseController.gameStateEnum _prevGameState;
    public void Open()
    {
        main._anim.Play("Open");
        SwitchTo(main);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _prevGameState = PlayerController.GameState;
        PlayerController.Instance.GameState_Change(BaseController.gameStateEnum.inactive);

        menuOpen = true;
    }
    public void Close()
    {
        main._anim.Play("Close");
        SwitchTo(main);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerController.Instance.GameState_Change(_prevGameState);

        menuOpen = false;
    }

    public void CustomizeButton()
    {
        SwitchTo(customize);
    }

    public void CustomizeSetup_Helmet()
    {
        CustomizeShow();
        customize.I_armorType = 0;
        ArmorManager.Instance.CreateHelmetUI(customize.G_subOptions.transform);
        GamepadSwitch();
    }
    public void CustomizeSetup_Chest()
    {
        CustomizeShow();
        customize.I_armorType = 1;
        ArmorManager.Instance.CreateChestUI(customize.G_subOptions.transform);
        GamepadSwitch();
    }
    public void CustomizeSetup_LArm()
    {
        CustomizeShow();
        customize.I_armorType = 2;
        ArmorManager.Instance.CreateArmUI(customize.G_subOptions.transform);
        GamepadSwitch();
    }
    public void CustomizeSetup_RArm()
    {
        CustomizeShow();
        customize.I_armorType = 3;
        ArmorManager.Instance.CreateArmUI(customize.G_subOptions.transform);
        GamepadSwitch();
    }
    public void CustomizeSetup_Legs()
    {
        CustomizeShow();
        customize.I_armorType = 4;
        ArmorManager.Instance.CreateLegUI(customize.G_subOptions.transform);
        GamepadSwitch();
    }
    public void CustomizeSetup_Back()
    {
        if (customize.G_listOptions.activeInHierarchy)
            BackButton();
        else
        {
            customize.G_listOptions.SetActive(true);
            customize.G_subOptions.SetActive(false);
            customize.G_subOptions.transform.DeleteChildren();
            GamepadSwitch(true);
        }
    }
    void CustomizeShow()
    {
        customize.G_listOptions.SetActive(false);
        customize.G_subOptions.SetActive(true);
        customize.G_subOptions.transform.DeleteChildren();
    }
    public void ChooseArmor(ArmorManager.ArmorClass _armor)
    {
        SaveData.equippedArmor[customize.I_armorType] = _armor._enum();
        _armor.Equip(PlayerController.Instance.RM_ragdoll, customize.I_armorType != 3);
    }

    public void LoadButton()
    {
        SwitchTo(load);
    }
    public void BackButton()
    {
        SwitchTo(main);
    }

    void ButtonHit()
    {
        if (!PlayerController.Instance.Inputs.b_isGamepad)
            EventSystem.current.SetSelectedGameObject(null);
    }
    public void GamepadSwitch(bool _back = false)
    {
        EventSystem.current.UpdateModules();
        if (menuOpen)
        {
            if (!PlayerController.Instance.Inputs.b_isGamepad)
                EventSystem.current.SetSelectedGameObject(null);
            else
                EventSystem.current.SetSelectedGameObject(_current.DefaultButton(_back));
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
