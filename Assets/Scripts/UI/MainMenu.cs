using PurrLobby;
using PurrNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    [SerializeField] private LobbyManager LM;
    public mainRefClass main;
    public customizeRefClass customize;
    public panelRefClass load;
    public panelRefClass settings;
    public storeRefClass store;
    public customizeLayoutRefClass customizeLayout;
    public customizeGraffitiRefClass customizeGraffiti;
    public loadingLevelRefClass loadingLevel;
    public endLevelRefClass endLevel;
    [Space(10)]
    public inputRefClass input;
    [Space(10)]
    public Volume V_postProcess;
    public AnimCurve AC_smooth;

    [HideInInspector] public panelRefClass current = null;

    private Vector3 v3_camLastLocalPos;
    private Vector3 v3_camMenuLocalPos;
    private Quaternion q_camLastLocalRot;

    public GameObject PF_equipParticle;

    private panelEnum openedPanel = panelEnum.main;
    public enum panelEnum { main, customize, store, load, settings, customizeLayout, loadLevel, endLevel, customizeGraffiti}

    [System.Serializable]
    public class panelRefClass
    {
        public Animator _anim;
        public Button _defaultButton;
        public Button _backButton;

        public bool cursorVisible = true;

        public virtual GameObject DefaultButton(bool _back = false)
        {
            if (_back)
                return _backButton.gameObject;
            return _defaultButton.gameObject;
        }
        public virtual void UpdateCurrency(int amt)
        {
            
        }
        public virtual void Open(AnimCurve _curve, Vector3 v3_camMenuLocalPos, Quaternion q_camLastLocalRot, bool _move = true)
        {
            _anim.SetBool("Open", true);
            if (_anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Close")
                _anim.PlayClip("Open");
            if (_move)
                Instance.StartCoroutine(PlayerManager.main.T_camHolder.GetChild(0).Move(v3_camMenuLocalPos, q_camLastLocalRot, true, 0.4f, _curve));
        }
        public virtual void Close()
        {
            _anim.SetBool("Open", false);
        }
        public virtual void OnUpdate(PlayerController _PC)
        {

        }
    }
    [System.Serializable]
    public class mainRefClass : panelRefClass
    {
        public TextMeshProUGUI TM_currency;
        public override void UpdateCurrency(int amt)
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
        public override void Open(AnimCurve _curve, Vector3 v3_camMenuLocalPos, Quaternion q_camLastLocalRot, bool _move = true)
        {
            base.Open(_curve, v3_camMenuLocalPos, q_camLastLocalRot, false);
            HideSubOptions();
            Transform _pivot = PlayerManager.main.T_camHookArmorEquip;
            if (_move)
                Instance.StartCoroutine(PlayerManager.main.T_camHolder.GetChild(0).Move(_pivot.position, _pivot.rotation, false, 0.4f, _curve));
        }
        public override void Close()
        {
            HideSubOptions();
            base.Close();
        }
        public void HideSubOptions()
        {
            G_listOptions.SetActive(true);
            G_subOptions.SetActive(false);
            G_subOptions.transform.DeleteChildren();
            Instance.GamepadSwitch(true);
        }
    }
    [System.Serializable]
    public class storeRefClass : panelRefClass
    {
        public ButtonGeneric PF_buttonTab;
        public ButtonGeneric PF_buttonGrid;
        public ButtonCost PF_buttonCost;

        public RectTransform RT_tabHolder;
        public RectTransform RT_gridHolder;
        public RectTransform RT_listHolder;

        public Image I_itemImage;
        public TextMeshProUGUI TM_itemName;
        public TextMeshProUGUI TM_itemRarity;
        public TextMeshProUGUI TM_itemDescription;

        public TextMeshProUGUI TM_cost;
        public TextMeshProUGUI TM_currency;
        [Space (10)]
        public TextMeshProUGUI TM_leftTab;
        public TextMeshProUGUI TM_rightTab;
        public TextMeshProUGUI TM_purchase;
        [Space(10)]

        public Image I_purchaseButton;

        public Sprite S_nullItem;
        public String S_nullName;
        public String S_nullRarity;
        public String S_nullDescription;

        public StoreManager Store;

        [HideInInspector] public StoreManager.enumType _activeTab = StoreManager.enumType.guns;
        [HideInInspector] public ItemClass activeItem = null;
        [HideInInspector] public bool canAfford = false;

        public override GameObject DefaultButton(bool _back = false)
        {
            if (_back)
                return _backButton.gameObject;
            else
            {
                if (RT_gridHolder.childCount > 0)
                    return RT_gridHolder.GetChild(0).gameObject;
                else
                    return RT_tabHolder.GetChild(0).gameObject;
            }
        }
        public void UpdateTabs()
        {
            RT_tabHolder.DeleteChildren();

            ButtonGeneric BG = Instantiate(PF_buttonTab, RT_tabHolder);
            BG.Setup(UpdateGrid_Guns, Store.gunIcon, Color.white);

            BG = Instantiate(PF_buttonTab, RT_tabHolder);
            BG.Setup(UpdateGrid_Armor, Store.armorIcon, Color.white);

            BG = Instantiate(PF_buttonTab, RT_tabHolder);
            BG.Setup(UpdateGrid_Consumable, Store.consumableIcon, Color.white);

            UpdateGrid_Guns();
        }
        public List<Action> ActionList()
        {
            return new List<Action>()
            {
                UpdateGrid_Guns,
                UpdateGrid_Armor,
                UpdateGrid_Consumable
            };
        }
        public void UpdateGrid_Guns()
        {
            _activeTab = StoreManager.enumType.guns;
            List<ItemClass> _items = new List<ItemClass>();
            foreach (ItemClass item in Store._guns)
            {
                if (item.ownedAmt == 0)
                    _items.Add(item);
            }
            UpdateGrid(_items);
        }
        public void UpdateGrid_Armor()
        {
            _activeTab = StoreManager.enumType.armor;
            List<ItemClass> _items = new List<ItemClass>();
            foreach (ItemClass item in Store._armor)
            {
                if (item.ownedAmt == 0)
                    _items.Add(item);
            }
            UpdateGrid(_items);
        }
        public void UpdateGrid_Consumable()
        {
            _activeTab = StoreManager.enumType.consumable;
            List<ItemClass> _items = new List<ItemClass>();
            foreach (ItemClass item in Store._consumable)
            {
                if (item.ownedAmt >= 0)
                    _items.Add(item);
            }
            UpdateGrid(_items);
        }
        public void UpdateGrid(StoreManager.enumType _enum)
        {
            switch (_enum)
            {
                case StoreManager.enumType.guns:
                    UpdateGrid_Guns();
                    break;
                case StoreManager.enumType.armor:
                    UpdateGrid_Armor();
                    break;
                default:
                    UpdateGrid_Guns();
                    break;
            }
        }
        public void UpdateGrid(List<ItemClass> _items)
        {
            RT_gridHolder.DeleteChildren();
            foreach (ItemClass _item in _items)
            {
                ButtonGeneric BG = Instantiate(PF_buttonGrid, RT_gridHolder);
                Sprite sprite = Sprite.Create(_item.image, new Rect(0, 0, _item.image.width, _item.image.height), new Vector2(_item.image.width / 2, _item.image.height / 2));
                BG.Setup(_item.OnClick, sprite, Color.white);
            }
            if (_items.Count > 0)
                UpdateItem(_items[0]);
            else
                NullItem();
        }
        public void UpdateItem(ItemClass _item)
        {
            if (_item == null)
            {
                NullItem();
                return;
            }
            activeItem = _item;
            Sprite sprite = Sprite.Create(_item.image, new Rect(0, 0, _item.image.width, _item.image.height), new Vector2(_item.image.width / 2, _item.image.height / 2));
            I_itemImage.sprite = sprite;

            TM_itemName.text = _item._name;
            TM_itemRarity.text = _item.GetEnumType().ToString();
            TM_itemDescription.text = _item._description;
            UpdateCost(_item);
        }
        public void NullItem()
        {
            activeItem = null;
            canAfford = false;
            I_itemImage.sprite = S_nullItem;

            TM_itemName.text = S_nullName;
            TM_itemRarity.text = S_nullRarity;
            TM_itemDescription.text = S_nullDescription;

            UpdateCurrency(SaveData.i_currency);
            TM_cost.color = Color.black;
            TM_cost.text = "N/A";
            RT_listHolder.DeleteChildren();
            I_purchaseButton.color = Color.grey;
        }

        void UpdateCost(ItemClass _item)
        {
            canAfford = true;
            UpdateCurrency(SaveData.i_currency);
            TM_cost.text = _item.cost.coinCost.ToString_Currency();
            if (SaveData.i_currency < _item.cost.coinCost)
            {
                canAfford = false;
                TM_cost.color = Color.red;
            }
            else
                TM_cost.color = Color.black;

            RT_listHolder.DeleteChildren();
            foreach (Resource.resourceClass _cost in _item.cost.list)
            {
                ButtonCost BC = Instantiate(PF_buttonCost, RT_listHolder);
                bool _temp = BC.Setup(_cost);
                canAfford = canAfford && _temp;
            }
            if (canAfford)
                I_purchaseButton.color = Color.green;
            else
                I_purchaseButton.color = Color.red;
        }
        public override void UpdateCurrency(int amt)
        {
            TM_currency.text = amt.ToString_Currency();
        }
        public override void Open(AnimCurve _curve, Vector3 v3_camMenuLocalPos, Quaternion q_camLastLocalRot, bool _move = true)
        {
            base.Open(_curve, v3_camMenuLocalPos, q_camLastLocalRot, false);
            UpdateTabs();
            Transform _pivot = PlayerManager.main.T_camHookStore;
            if ( _move)
                Instance.StartCoroutine(PlayerManager.main.T_camHolder.GetChild(0).Move(_pivot.position, _pivot.rotation, false, 0.4f, _curve));
        }
    }
    [System.Serializable]
    public class customizeLayoutRefClass : panelRefClass
    {
        public LayoutCustomize layoutCustomize;

        public TextMeshProUGUI TM_buildButton;
        public TextMeshProUGUI TM_backButton;

        public override void Open(AnimCurve _curve, Vector3 v3_camMenuLocalPos, Quaternion q_camLastLocalRot, bool _move = true)
        {
            layoutCustomize.Display();
            base.Open(_curve, v3_camMenuLocalPos, q_camLastLocalRot, _move);
        }
        public override void Close()
        {
            layoutCustomize.Hide();
            base.Close();
        }
        public override void OnUpdate(PlayerController _PC)
        {
            layoutCustomize.OnUpdate(_PC);
        }
    }
    [System.Serializable]
    public class customizeGraffitiRefClass : panelRefClass
    {
        public GraffitiCustomize graffitiCustomize;
        public GraffitiManager graffitiManager;

        public TextMeshProUGUI TM_buildButton;
        public TextMeshProUGUI TM_backButton;


        public override void Open(AnimCurve _curve, Vector3 v3_camMenuLocalPos, Quaternion q_camLastLocalRot, bool _move = true)
        {
            graffitiCustomize.Display();
            base.Open(_curve, v3_camMenuLocalPos, q_camLastLocalRot, _move);
        }
        public override void Close()
        {
            base.Close();
        }
        public override void OnUpdate(PlayerController _PC)
        {
            graffitiCustomize.OnUpdate(_PC);
        }
    }
    [System.Serializable]
    public class loadingLevelRefClass : panelRefClass
    {
        [Header("Prefabs")]
        public LoadingArea PF_loadingEnv;
        [HideInInspector] public LoadingArea G_loadedEnv = null;
        [Header("Text")]
        public TextMeshProUGUI TM_area;
        public TextMeshProUGUI TM_missionName;
        public TextMeshProUGUI TM_missionDescription;
        public TextMeshProUGUI TM_mainObjective;
        public TextMeshProUGUI TM_sideObjective;
        public TextMeshProUGUI TM_loadingText;
        [Header("NameTags")]
        public UI_NameTag[] UI_nameTags;


        public override void Open(AnimCurve _curve, Vector3 v3_camMenuLocalPos, Quaternion q_camLastLocalRot, bool _move = true)
        {
            if (G_loadedEnv != null)
                Destroy(G_loadedEnv);
            G_loadedEnv = Instantiate(PF_loadingEnv, Vector3.down * 1000, Quaternion.identity);
            G_loadedEnv.Setup();
            SetMissionText();


            base.Open(_curve, v3_camMenuLocalPos, q_camLastLocalRot, _move);
        }
        public override void Close()
        {
            if (G_loadedEnv != null)
                Destroy(G_loadedEnv);
            base.Close();
        }
        public override void OnUpdate(PlayerController _PC)
        {
            base.OnUpdate(_PC);
        }
        void SetMissionText()
        {
            Mission _mission = SaveData.missionCurrent;
            if (_mission == null) return;

            TM_missionName.text = _mission._name;
            TM_missionDescription.text = _mission._description;
            TM_mainObjective.text = _mission._steps[0]._objective.GetDescription_Long();
            TM_sideObjective.text = _mission._sideObjective.GetDescription_Long();
        }
        public void SetNames(string[] _names)
        {
            //Reorder to match seating if applicable
            if (_names.Length >= 3)
            {
                string _temp = _names[2];
                _names[2] = _names[1];
                _names[1] = _temp;
            }
            //Show Names
            for (int i = 0; i < UI_nameTags.Length; i++)
            {
                bool _valid = i < _names.Length;
                UI_nameTags[i].gameObject.SetActive(_valid);
                if (!_valid) continue;
                UI_nameTags[i].Setup(_names[i]);
            }
        }
    }
    [System.Serializable]
    public class endLevelRefClass : panelRefClass
    {
        [Header("Prefabs")]
        public LoadingArea PF_loadingEnv;
        [HideInInspector] public LoadingArea G_loadedEnv = null;
        public UI_Resource PF_UIResource;
        [Header("Object References")]
        public UI_Resource UI_money;
        public RectTransform RT_resourceHolder;
        [Header("Text")]
        public TextMeshProUGUI TM_area;
        public TextMeshProUGUI TM_missionName;
        public TextMeshProUGUI TM_mainObjective;
        public TextMeshProUGUI TM_sideObjective;
        public TextMeshProUGUI TM_loadingText;
        public TextMeshProUGUI TM_timerText;
        [Header("Images")]
        public Image I_missionIcon;
        public GameObject G_missionFailedIcon;
        [Header("NameTags")]
        public UI_NameTag[] UI_nameTags;
        [HideInInspector] public Themes.themeEnum themeEnum;
        [HideInInspector] public PlayerController.StatClass stats;
        private Coroutine closeMenu = null;
        public override void Open(AnimCurve _curve, Vector3 v3_camMenuLocalPos, Quaternion q_camLastLocalRot, bool _move = true)
        {
            if (G_loadedEnv != null)
                Destroy(G_loadedEnv);
            G_loadedEnv = Instantiate(PF_loadingEnv, Vector3.down * 1000, Quaternion.identity);
            G_loadedEnv.Setup();
            SetMissionText();

            if (closeMenu != null) MainMenu.Instance.StopCoroutine(closeMenu);
            closeMenu = MainMenu.Instance.StartCoroutine(CloseMenu());

            base.Open(_curve, v3_camMenuLocalPos, q_camLastLocalRot, _move);
        }
        public override void Close()
        {
            if (closeMenu != null) MainMenu.Instance.StopCoroutine(closeMenu);

            if (G_loadedEnv != null)
                Destroy(G_loadedEnv);
            base.Close();
        }
        public override void OnUpdate(PlayerController _PC)
        {
            base.OnUpdate(_PC);
        }
        void SetMissionText()
        {
            Mission _mission = SaveData.missionCurrent;
            if (_mission == null) return;

            TM_missionName.text = _mission._name;
            if (stats != null)
                TM_timerText.text = stats.GetRunTime().ToString_Duration(true);

            if (SaveData.objectives.Count > 0) TM_mainObjective.text = SaveData.objectives[0].GetDescription_Long();
            else TM_mainObjective.text = "";
            if (SaveData.objectives.Count > 1) TM_sideObjective.text = SaveData.objectives[1].GetDescription_Long();
            else TM_sideObjective.text = "";

            DisplayMoneyResources();
        }

        void DisplayMoneyResources()
        {
            UI_money.SetMoney(LevelGen_Holder.Instance.GetCollectedValue(), SaveData.i_currency);
            foreach (var item in PlayerManager.Instance.runData.resources)
            {
                UI_Resource _temp = Instantiate(PF_UIResource, RT_resourceHolder);
                _temp.SetResource(item);
            }
        }
        public void SetNames(string[] _names)
        {
            SetNames(_names, PlayerManager.Instance.Players);
        }
        public void SetNames(string[] _names, List<BaseInfo> _players)
        {
            //Show Names
            for (int i = 0; i < UI_nameTags.Length; i++)
            {
                bool _valid = i < _players.Count && i < _names.Length;
                UI_nameTags[i].gameObject.SetActive(_valid);
                if (!_valid) continue;
                UI_nameTags[i].Setup(_names[i], "Champion", ArmorManager.Instance.GetArmorType(_players[i].icon).image, _players[i].b_alive);
            }
        }

        public IEnumerator CloseMenu()
        {
            yield return new WaitForSecondsRealtime(1f);
            while (!UnityEngine.Input.anyKey)
            {
                yield return new WaitForEndOfFrame();
            }
            LevelGen_Holder.LoadTheme(themeEnum);
        }
    }
    [System.Serializable]
    public class inputRefClass
    {
        public TMP_SpriteAsset SA_keyboard;
        public TMP_SpriteAsset SA_xbox;
        public TMP_SpriteAsset SA_ps;
        public TMP_SpriteAsset SA_switch;
        public TMP_SpriteAsset SA_steamDeck;
    }
    public void SwitchTo(panelRefClass GO)
    {
        current = GO;
        if (main != GO) main.Close();
        if (customize != GO) customize.Close();
        if (load != GO) load.Close();
        if (store != GO) store.Close();
        if (settings != GO) settings.Close();
        if (customizeLayout != GO) customizeLayout.Close();
        if (loadingLevel != GO) loadingLevel.Close();
        if (customizeGraffiti != GO) customizeGraffiti.Close();

        GO.Open(AC_smooth, v3_camMenuLocalPos, q_camLastLocalRot);

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
    
    #region Customize
    public void CustomizeButton()
    {
        SwitchTo(customize);
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
    void CustomizeShow()
    {
        customize.G_listOptions.SetActive(false);
        customize.G_subOptions.SetActive(true);
        customize.G_subOptions.transform.DeleteChildren();
    }
    public void ChooseArmor(ArmorPiece _armor)
    {
        SaveData.equippedArmor[customize.I_armorType] = _armor.GetEnum();
        _armor.Equip(PlayerManager.main.RM_ragdoll, customize.I_armorType != 3);
        _armor.AssignToPlayer(customize.I_armorType != 3);
        EquipParticles(_armor.Hooks(PlayerManager.main.RM_ragdoll));
    }
    void EquipParticles(Transform[] _points)
    {
        foreach (Transform t in _points)
        {
            GameObject GO = GameObject.Instantiate(PF_equipParticle, t.position, t.rotation);
        }
    }
    #endregion
    #region Store
    public void PurchaseItem()
    {
        if (store.canAfford)
        {
            store.activeItem.Purchase();
            store.UpdateGrid(store._activeTab);
            Invoke(nameof(CheckSelectedButton), 0.01f);
        }
    }
    #endregion
    #region Load
    public void LoadButton()
    {
        SwitchTo(load);
    }
    #endregion
    #region Settings
    public void SettingsButton()
    {
        SwitchTo(settings);
    }
    #endregion
    #region Reload
    public void ReloadButton()
    {
        int _id = -1;
        if (SaveData.missionCurrent != null)
            _id = SaveData.missionCurrent._id;
        LevelGen_Holder.LoadTheme(SaveData.themeCurrent, _id);
    }
    #endregion
    #region CustomizeLayout
    public void CustomizeLayoutButton()
    {
        SwitchTo(customizeLayout);
    }
    #endregion
    #region General
    public void Menu_Tapped()
    {
        if (menuOpen) Close();
        else Open();
    }

    private PlayerController.gameStateEnum _prevGameState;
    public void Open(panelEnum _enum = panelEnum.main)
    {
        openedPanel = _enum;
        switch (_enum)
        {
            case panelEnum.main: Open(main); break;
            case panelEnum.customize: Open(customize, 1f); break;
            case panelEnum.store: Open(store, 1f); break;
            case panelEnum.load: Open(load); break;
            case panelEnum.settings: Open(settings); break;
            case panelEnum.customizeLayout: Open(customizeLayout); break;
            case panelEnum.customizeGraffiti: Open(customizeGraffiti); break;
            case panelEnum.loadLevel: LoadLevel(); break;
            case panelEnum.endLevel: LoadEndLevel(); break;
            default: Open(main); break;
        }
    }
    public void Open(panelRefClass _panel, float _timeScale = 0f)
    {
        menuOpen = true;
        _panel.UpdateCurrency(SaveData.i_currency);
        v3_camLastLocalPos = PlayerManager.main.T_camHolder.GetChild(0).localPosition;
        q_camLastLocalRot = PlayerManager.main.T_camHolder.GetChild(0).localRotation;

        v3_camMenuLocalPos = v3_camLastLocalPos;
        v3_camMenuLocalPos.x = -v3_camMenuLocalPos.x;

        StartCoroutine(V_postProcess.Fade(true));

        _panel._anim.Play("Open");
        SwitchTo(_panel);
        TimeManager.TimeScale.value = _timeScale;

        _prevGameState = PlayerManager.main.GameState;
        PlayerManager.main.GameState_Change(PlayerController.gameStateEnum.menu);
    }
    public void LoadLevel()
    {
        loadingLevel.UpdateCurrency(SaveData.i_currency);
        loadingLevel._anim.Play("Open");
        loadingLevel.Open(AC_smooth, v3_camMenuLocalPos, q_camLastLocalRot, false);
    }
    public void LoadEndLevel()
    {
        endLevel.UpdateCurrency(SaveData.i_currency);
        endLevel._anim.Play("Open");
        endLevel.Open(AC_smooth, v3_camMenuLocalPos, q_camLastLocalRot, false);
    }
    public void Close()
    {
        current.Close();
        current._anim.Play("Close");
        StartCoroutine(PlayerManager.main.T_camHolder.GetChild(0).Move(v3_camLastLocalPos, q_camLastLocalRot, true, 0.2f, AC_smooth, CloseFinish));
        StartCoroutine(V_postProcess.Fade(false));
    }
    public void BackButton()
    {
        int close = 1;
        if (current == customize)
        {
            if (customize.G_subOptions.activeInHierarchy)
            {
                customize.HideSubOptions();
                close = 0;
            }
            else if (openedPanel == panelEnum.customize)
                close = 2;
        }
        if (current == store)
            close = 2;
        if (current == customizeLayout)
            close = 2;
        if (current == customizeGraffiti)
            close = 2;

        if (close == 1)
            SwitchTo(main);
        if (close == 2)
            Close();
    }
    public void Tab_Switch(bool _left)
    {
        if (menuOpen)
        {
            if (current == store)
            {
                List<Action> _actions = store.ActionList();
                int _tab = Mathf.Clamp((int)store._activeTab + (_left ? -1 : 1), 0, _actions.Count - 1);
                _actions[_tab].Invoke();
                Invoke(nameof(CheckSelectedButton), 0.01f);
            }
            if (current == customizeLayout)
            {
                if (_left) LayoutCustomize.Instance.Rotate(1);
                else LayoutCustomize.Instance.Rotate(-1);
            }
        }
    }
    public void BuildMenu()
    {
        if (menuOpen)
        {
            if (current == customizeLayout)
                LayoutCustomize.Instance.BuildMenu();
            if (current == customizeGraffiti)
                GraffitiCustomize.Instance.BuildMenu();
        }
    }
    public void CheckSelectedButton()
    {
        if (PlayerManager.main.Inputs.b_isGamepad)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
                EventSystem.current.SetSelectedGameObject(current.DefaultButton(false));
            else if (!EventSystem.current.currentSelectedGameObject.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(current.DefaultButton(false));
        }
    }
    public void Purchase_Pressed()
    {
        if (menuOpen)
        {
            if (current == store)
            {
                PurchaseItem();
            }
        }
    }
    public void GamepadSwitch(bool _back = false)
    {
        if (menuOpen)
        {
            EventSystem.current.UpdateModules();
            if (!PlayerManager.main.Inputs.b_isGamepad)
            {
                EventSystem.current.SetSelectedGameObject(null);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = current.cursorVisible;
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(current.DefaultButton(_back));
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        GamepadUpdate();
    }

    public void GamepadUpdate()
    {
        store.TM_leftTab.text = "".ToString_Input(PlayerController.inputActions.LeftTab, store.TM_leftTab, Interactable.enumType.input);
        store.TM_rightTab.text = "".ToString_Input(PlayerController.inputActions.RightTab, store.TM_rightTab, Interactable.enumType.input);

        store.TM_purchase.text = "Get".ToString_Input(PlayerController.inputActions.Purchase, store.TM_purchase, Interactable.enumType.combine);
        
        customizeLayout.TM_buildButton.text = "Build".ToString_Input(PlayerController.inputActions.BuildMenu, customizeLayout.TM_buildButton, Interactable.enumType.combine);
        customizeLayout.TM_backButton.text = "Back".ToString_Input(PlayerController.inputActions.Back, customizeLayout.TM_backButton, Interactable.enumType.combine);

        customizeGraffiti.TM_buildButton.text = "Build".ToString_Input(PlayerController.inputActions.BuildMenu, customizeGraffiti.TM_buildButton, Interactable.enumType.combine);
        customizeGraffiti.TM_backButton.text = "Back".ToString_Input(PlayerController.inputActions.Back, customizeGraffiti.TM_backButton, Interactable.enumType.combine);
    }

    
    void CloseFinish()
    {
        TimeManager.TimeScale.value = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerManager.main.GameState_Change(_prevGameState);

        menuOpen = false;
    }

    public void Quit()
    {
        TimeManager.TimeScale.value = 1;
        LM.LeaveLobby();
        if (NetworkManager.main.isServer)
            NetworkManager.main.StopServer();
        NetworkManager.main.StopClient();
        SceneManager.LoadScene(0);
    }
    #endregion
}
