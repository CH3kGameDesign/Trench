using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class GraffitiSelect : MonoBehaviour
{
    public static GraffitiSelect Instance;
    public RawImage I_graffiti;
    public RectTransform RT_tabs;
    public RectTransform RT_grid;
    public TextMeshProUGUI TM_description;
    public TextMeshProUGUI TM_controls;
    public ButtonAdvanced PF_button;
    public ButtonGeneric PF_tabButton;

    public defaultStateClass State_NoneSelected;
    public defaultStateClass State_NewGraffiti;

    [System.Serializable]
    public class defaultStateClass
    {
        public Texture i_image;
        public string s_description;
    }

    [Space(10)]
    public GameObject G_GraffitiSelect;
    public GameObject G_GraffitiCustomize;

    GraffitiManager.graffitiClass _activeGraffiti;
    GraffitiManager.graffitiTypeEnum _activeTab;

    List<tabClass> tabs = new List<tabClass>();
    public class tabClass
    {
        public string S_tabName = "";
        public GraffitiManager.graffitiTypeEnum T_type;
        public List<graffitiItem> gridButtons = new List<graffitiItem>();
        public ButtonGeneric tabButton;
        public GraffitiSelect GS;

        float f_Width = 100;
        float f_WidthScale = 1.5f;

        public tabClass(GraffitiManager.graffitiTypeEnum _type, GraffitiSelect _GS)
        {
            T_type = _type;
            GS = _GS;
            CreateTab();
        }
        public void UpdateList()
        {
            List<GraffitiManager.graffitiClass> _graffiti;
            switch (T_type)
            {
                case GraffitiManager.graffitiTypeEnum.tags:
                    _graffiti = SaveData.graffitiTags;
                    break;
                case GraffitiManager.graffitiTypeEnum.armor:
                    _graffiti = SaveData.graffitiArmor;
                    break;
                case GraffitiManager.graffitiTypeEnum.ships:
                    _graffiti = SaveData.graffitiShips;
                    break;
                default:
                    Debug.LogError("Non-specified Enum Type");
                    return;
            }
            CreateList(_graffiti);
        }
        void CreateList(List<GraffitiManager.graffitiClass> _graffiti)
        {
            foreach (var item in gridButtons) { GameObject.Destroy(item.button.gameObject); }
            gridButtons.Clear();
            gridButtons.Add(new graffitiItem(null, GS));
            foreach (var item in _graffiti)
                gridButtons.Add(new graffitiItem(item, GS));
        }
        void CreateTab()
        {
            switch (T_type)
            {
                case GraffitiManager.graffitiTypeEnum.tags: S_tabName = "Tags"; break;
                case GraffitiManager.graffitiTypeEnum.armor: S_tabName = "Armor"; break;
                case GraffitiManager.graffitiTypeEnum.ships: S_tabName = "Ships"; break;
                default:
                    Debug.LogError("Non-specified Enum Type");
                    return;
            }

            tabButton = Instantiate(GS.PF_tabButton, GS.RT_tabs);
            tabButton.Setup(OnClick, null, null, S_tabName);
            f_Width = tabButton.GetComponent<RectTransform>().rect.width;
        }
        public void OnClick() { GS.SwapTabs(T_type); }
        public void SetActive(GraffitiManager.graffitiTypeEnum _type) { SetActive(_type == T_type); }
        public void SetActive(bool _active)
        {
            foreach (var item in gridButtons)
                item.button.gameObject.SetActive(_active);
                
            if (_active)    tabButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                                f_Width * f_WidthScale);
            else            tabButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                                f_Width);
        }
    }
    public class graffitiItem
    {
        public GraffitiManager.graffitiClass graffiti;
        public ButtonAdvanced button;
        GraffitiSelect GS;

        public graffitiItem(GraffitiManager.graffitiClass _g, GraffitiSelect _GS)
        {
            graffiti = _g;
            GS = _GS;

            button = Instantiate(GS.PF_button, GS.RT_grid);
            Texture2D T2D = null;
            if (graffiti != null) T2D = graffiti.GetTexture();
            button.Setup(OnClick, OnSelect, OnDeselect, T2D, Color.white);
        }
        public void OnClick() { GS.Graffiti_Confirm(graffiti); }
        public void OnSelect() { GS.Graffiti_Select(graffiti); }
        public void OnDeselect() { GS.Graffiti_Deselect(graffiti); }
    }

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupTabs();
    }

    void SetupTabs()
    {
        tabs.Clear();
        tabs.Add(new tabClass(GraffitiManager.graffitiTypeEnum.tags, this));
        tabs.Add(new tabClass(GraffitiManager.graffitiTypeEnum.armor, this));
        tabs.Add(new tabClass(GraffitiManager.graffitiTypeEnum.ships, this));
        SwapTabs(GraffitiManager.graffitiTypeEnum.tags);
    }
    
    public void UpdateTabs()
    {
        foreach (var item in tabs)
            item.UpdateList();
        SwapTabs(_activeTab);
    }

    void SwapTabs(GraffitiManager.graffitiTypeEnum _type)
    {
        _activeTab = _type;
        foreach (var item in tabs)
            item.SetActive(_type);
    }
    public void SwapTabs(int _num)
    {
        int _i = 0;
        for (int i = 0; i < tabs.Count; i++)
            if (tabs[i].T_type == _activeTab)
                _i = Mathf.Clamp(i + _num, 0, tabs.Count);
        SwapTabs(tabs[_i].T_type);
    }

    void Graffiti_Confirm(GraffitiManager.graffitiClass _g)
    {
        GraffitiCustomize.Instance.LoadSave(_g);
        ShowSelectScreen(false);
    }

    public void ShowSelectScreen(bool _show)
    {
        if (_show) UpdateTabs();

        G_GraffitiCustomize.SetActive(!_show);
        G_GraffitiSelect.SetActive(_show);
    }
    void Graffiti_Select(GraffitiManager.graffitiClass _g)
    {
        _activeGraffiti = _g;

        if (_g == null)
        {
            I_graffiti.texture = State_NewGraffiti.i_image;
            TM_description.text = State_NewGraffiti.s_description;
        }
        else
        {
            I_graffiti.texture = _g.GetTexture();
            TM_description.text = _g._name;
        }
    }
    void Graffiti_Deselect(GraffitiManager.graffitiClass _g)
    {
        if (_activeGraffiti != _g)
            return;

        I_graffiti.texture = State_NoneSelected.i_image;
        TM_description.text = State_NoneSelected.s_description;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeTab(int _dir)
    {
        
    }
}
