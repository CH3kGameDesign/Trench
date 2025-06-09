using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;
    public mainRefClass main;
    public customizeRefClass customize;
    public panelRefClass load;
    public storeRefClass store;

    public Volume V_postProcess;
    public AnimCurve AC_smooth;

    private panelRefClass _current = null;

    private Vector3 v3_camLastLocalPos;
    private Vector3 v3_camMenuLocalPos;
    private Quaternion q_camLastLocalRot;

    public GameObject PF_equipParticle;

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
    public class mainRefClass : panelRefClass
    {
        public TextMeshProUGUI TM_currency;
        public void UpdateCurrency(int amt)
        {
            TM_currency.text = amt.ToString_Currency();
        }
    }

    [System.Serializable]
    public class customizeRefClass : panelRefClass
    {
        [HideInInspector] public int I_armorType = 0;
        [HideInInspector] public ArmorOptionButton _equipped;
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
    [System.Serializable]
    public class storeRefClass : panelRefClass
    {
        public ButtonGeneric PF_buttonTab;
        public ButtonGeneric PF_buttonGrid;

        public RectTransform RT_tabHolder;
        public RectTransform RT_gridHolder;
        public RectTransform RT_listHolder;

        public Image I_itemImage;
        public TextMeshProUGUI TM_itemName;
        public TextMeshProUGUI TM_itemRarity;
        public TextMeshProUGUI TM_itemDescription;

        public TextMeshProUGUI TM_cost;
        public TextMeshProUGUI TM_currency;

        public StoreManager Store;

        public override GameObject DefaultButton(bool _back = false)
        {
            if (_back)
                return _backButton.gameObject;
            else
                return _defaultButton.gameObject;
        }

        public void UpdateTabs()
        {
            RT_tabHolder.DeleteChildren();

            ButtonGeneric BG = Instantiate(PF_buttonTab, RT_tabHolder);
            BG.Setup(UpdateGrid_Guns, Store.gunIcon, Color.white);

            BG = Instantiate(PF_buttonTab, RT_tabHolder);
            BG.Setup(UpdateGrid_Armor, Store.armorIcon, Color.white);
        }
        public void UpdateGrid_Guns()
        {
            List<ItemClass> _items = new List<ItemClass>();
            _items.AddRange(Store._guns);
            UpdateGrid(_items);
        }
        public void UpdateGrid_Armor()
        {
            List<ItemClass> _items = new List<ItemClass>();
            _items.AddRange(Store._armor);
            UpdateGrid(_items);
        }
        public void UpdateGrid(List<ItemClass> _items)
        {
            RT_gridHolder.DeleteChildren();
            foreach (ItemClass _item in _items)
            {
                ButtonGeneric BG = Instantiate(PF_buttonGrid, RT_gridHolder);
                Sprite sprite = Sprite.Create(_item.image, new Rect(0, 0, _item.image.width, _item.image.height), new Vector2(_item.image.width / 2, _item.image.height / 2));
                BG.Setup(UpdateGrid_Guns, sprite, Color.white);
            }
        }
        public void UpdateItem(ItemClass _item)
        {

        }
    }
    public void SwitchTo(panelRefClass GO)
    {
        if (_current == customize)
            CustomizeSetup_Close();

        _current = GO;
        main._anim.SetBool("Open", main == GO);
        customize._anim.SetBool("Open", customize == GO);
        load._anim.SetBool("Open", load == GO);
        store._anim.SetBool("Open", load == GO);

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
        menuOpen = true;
        main.UpdateCurrency(SaveData.i_currency);
        v3_camLastLocalPos = PlayerController.Instance.T_camHolder.GetChild(0).localPosition;
        q_camLastLocalRot = PlayerController.Instance.T_camHolder.GetChild(0).localRotation;

        v3_camMenuLocalPos = v3_camLastLocalPos;
        v3_camMenuLocalPos.x = -v3_camMenuLocalPos.x;
        StartCoroutine(PlayerController.Instance.T_camHolder.GetChild(0).Move(v3_camMenuLocalPos, q_camLastLocalRot, true, 0.4f, AC_smooth));

        StartCoroutine(V_postProcess.Fade(true));

        main._anim.Play("Open");
        SwitchTo(main);
        Time.timeScale = 0;

        _prevGameState = PlayerController.GameState;
        PlayerController.Instance.GameState_Change(BaseController.gameStateEnum.menu);

    }
    public void Close()
    {
        main._anim.Play("Close");
        SwitchTo(main);
        StartCoroutine(PlayerController.Instance.T_camHolder.GetChild(0).Move(v3_camLastLocalPos, q_camLastLocalRot, true, 0.2f, AC_smooth, CloseFinish));
        StartCoroutine(V_postProcess.Fade(false));
    }

    public void CustomizeButton()
    {
        SwitchTo(customize);
        Transform _pivot = PlayerController.Instance.T_camHookArmorEquip;
        StartCoroutine(PlayerController.Instance.T_camHolder.GetChild(0).Move(_pivot.position, _pivot.rotation, false, 0.4f, AC_smooth));
    }

    public void CustomizeSetup_Helmet()
    {
        CustomizeShow();
        customize.I_armorType = 0;
        Armor_Type _enum = SaveData.equippedArmor[customize.I_armorType];
        ArmorManager.Instance.CreateHelmetUI(customize.G_subOptions.transform, _enum);
        GamepadSwitch();
    }
    public void CustomizeSetup_Chest()
    {
        CustomizeShow();
        customize.I_armorType = 1;
        Armor_Type _enum = SaveData.equippedArmor[customize.I_armorType];
        ArmorManager.Instance.CreateChestUI(customize.G_subOptions.transform, _enum);
        GamepadSwitch();
    }
    public void CustomizeSetup_LArm()
    {
        CustomizeShow();
        customize.I_armorType = 2;
        Armor_Type _enum = SaveData.equippedArmor[customize.I_armorType];
        ArmorManager.Instance.CreateArmUI(customize.G_subOptions.transform, _enum);
        GamepadSwitch();
    }
    public void CustomizeSetup_RArm()
    {
        CustomizeShow();
        customize.I_armorType = 3;
        Armor_Type _enum = SaveData.equippedArmor[customize.I_armorType];
        ArmorManager.Instance.CreateArmUI(customize.G_subOptions.transform, _enum);
        GamepadSwitch();
    }
    public void CustomizeSetup_Legs()
    {
        CustomizeShow();
        customize.I_armorType = 4;
        Armor_Type _enum = SaveData.equippedArmor[customize.I_armorType];
        ArmorManager.Instance.CreateLegUI(customize.G_subOptions.transform, _enum);
        GamepadSwitch();
    }
    public void CustomizeSetup_Materials()
    {
        CustomizeShow();
        customize.I_armorType = 5;
        Armor_Type _enum = SaveData.equippedArmor[customize.I_armorType];
        ArmorManager.Instance.CreateMaterialUI(customize.G_subOptions.transform, _enum);
        GamepadSwitch();
    }
    void CustomizeSetup_Close()
    {
        customize.G_listOptions.SetActive(true);
        customize.G_subOptions.SetActive(false);
        customize.G_subOptions.transform.DeleteChildren();
        GamepadSwitch(true);
    }
    void CustomizeShow()
    {
        customize.G_listOptions.SetActive(false);
        customize.G_subOptions.SetActive(true);
        customize.G_subOptions.transform.DeleteChildren();
    }
    public void ChooseArmor(ArmorPiece _armor)
    {
        SaveData.equippedArmor[customize.I_armorType] = _armor._enum();
        _armor.Equip(PlayerController.Instance.RM_ragdoll, customize.I_armorType != 3);
        EquipParticles(_armor.Hooks(PlayerController.Instance.RM_ragdoll));
    }
    void EquipParticles(Transform[] _points)
    {
        foreach (Transform t in _points)
        {
            GameObject GO = GameObject.Instantiate(PF_equipParticle, t.position, t.rotation);
        }
    }

    public void LoadButton()
    {
        SwitchTo(load);
    }
    public void BackButton()
    {
        if (customize.G_subOptions.activeInHierarchy)
            CustomizeSetup_Close();
        else
        {
            SwitchTo(main);
            StartCoroutine(PlayerController.Instance.T_camHolder.GetChild(0).Move(v3_camMenuLocalPos, q_camLastLocalRot, true, 0.4f, AC_smooth));
        }
    }

    void ButtonHit()
    {
        if (!PlayerController.Instance.Inputs.b_isGamepad)
            EventSystem.current.SetSelectedGameObject(null);
    }
    public void GamepadSwitch(bool _back = false)
    {
        if (menuOpen)
        {
            EventSystem.current.UpdateModules();
            if (!PlayerController.Instance.Inputs.b_isGamepad)
            {
                EventSystem.current.SetSelectedGameObject(null);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(_current.DefaultButton(_back));
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    
    void CloseFinish()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerController.Instance.GameState_Change(_prevGameState);

        menuOpen = false;
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
