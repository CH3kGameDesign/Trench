using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.DebugUI.Table;

public class RadialMenu : MonoBehaviour
{
    [System.Serializable]
    public class RefClass
    {
        public GameObject G_radialMenu;
        public GameObject G_radialSubBG;
        public GameObject PF_radialItem;
        public RectTransform RT_radialHolder;
        public RectTransform RT_radialHolder_Sub;
        public RectTransform RT_cursor;
        public LineRendererUI LR_cursor;
        [HideInInspector] public RectTransform[] rt_radialItems = new RectTransform[0];
        [HideInInspector] public List<RectTransform[]> rt_radialItems_Sub = new List<RectTransform[]>();
    }
    public RefClass Ref = new RefClass();

    [System.Serializable]
    public class ItemInfoRefClass
    {
        public RectTransform RT_holder;
        public RawImage I_itemSprite_Weapon;
        public RawImage I_itemSprite_Item;

        public TextMeshProUGUI TM_name;
        public TextMeshProUGUI TM_type;
        public TextMeshProUGUI TM_ammo;

        public TextMeshProUGUI TM_primary;
        public TextMeshProUGUI TM_flavour;
        public GameObject G_Functions;
        [Space(10)]
        public TextMeshProUGUI TM_Trigger1;
        public TextMeshProUGUI TM_Description1;

        public TextMeshProUGUI TM_Trigger2;
        public TextMeshProUGUI TM_Description2;

        public TextMeshProUGUI TM_Trigger3;
        public TextMeshProUGUI TM_Description3;

        public void Display(GunClass _gun)
        {
            RT_holder.gameObject.SetActive(true);
            G_Functions.SetActive(true);
            RT_holder.sizeDelta = new Vector2(300, 520);
            TM_name.text = _gun._name;
            TM_type.text = "Weapon";
            TM_ammo.text = _gun.clipAmmo.ToString_Clip() + "<size=20><color=#B0B0B0>" + _gun.clipVariables.clipSize +"</color></size>";

            TM_primary.text = _gun.functionText.primary;
            TM_flavour.text = _gun._description;

            TM_Trigger1.text = _gun.functionText.trigger1;
            TM_Description1.text = _gun.functionText.description1;

            TM_Trigger2.text = _gun.functionText.trigger2;
            TM_Description2.text = _gun.functionText.description2;

            TM_Trigger3.text = _gun.functionText.trigger3;
            TM_Description3.text = _gun.functionText.description3;

            I_itemSprite_Weapon.texture = _gun.image;
            I_itemSprite_Weapon.gameObject.SetActive(true);
            I_itemSprite_Item.gameObject.SetActive(false);
        }
        public void Display(Consumable.save _item)
        {
            Item_Consumable _itemC = Consumable.GetConsumableType_Static(_item._type);

            RT_holder.gameObject.SetActive(true);
            G_Functions.SetActive(false);
            RT_holder.sizeDelta = new Vector2(300, 220);
            TM_name.text = _itemC._name;
            TM_type.text = "Item";
            TM_ammo.text = _item._amt.ToString_Clip() + "<size=20><color=#B0B0B0>" + _item._totalAmt + "</color></size>";

            TM_primary.text = _itemC.primary;
            TM_flavour.text = _itemC._description;

            TM_Trigger1.text = "";
            TM_Description1.text = "";

            TM_Trigger2.text = "";
            TM_Description2.text = "";

            TM_Trigger3.text = "";
            TM_Description3.text = "";

            I_itemSprite_Item.texture = _itemC.image;
            I_itemSprite_Weapon.gameObject.SetActive(false);
            I_itemSprite_Item.gameObject.SetActive(true);
        }
    }
    public ItemInfoRefClass ItemInfoRef = new ItemInfoRefClass();

    [System.Serializable]
    public class ValueClass
    {
        [HideInInspector] public int i_childAmt = 0;
        [HideInInspector] public int i_selChild = -1;
        [HideInInspector] public int i_selSubChild = -1;
        [HideInInspector] public int i_lastChild = -1;
        [HideInInspector] public int i_lastSubChild = -1;

        public float F_radial_BaseDistance = 175;
        public float F_radial_SelDistance = 250;
        [Space(10)]
        public float F_radialSub_BaseDistance = 175;
        public float F_radialSub_SelDistance = 250;
        [Space(10)]
        public float F_subGroupRange = 180;
        [Space(10)]
        public float F_sensitivity = 100;
        public float F_deadzone = 75;
        public float F_itemSelectionGapPercent = 0.1f;
        [Space(10)]
        public float F_coyoteTime = 0.1f;
        [HideInInspector] public Coroutine C_CoyoteTime = null;
    }
    public ValueClass Values = new ValueClass();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(int[] _childAmt)
    {
        ClearChildren();
        Values.i_childAmt = _childAmt.Length;
        Ref.rt_radialItems = new RectTransform[_childAmt.Length];
        float _rot = 0;
        float _rotIncrease = 360 / _childAmt.Length;
        for (int i = 0; i < _childAmt.Length; i++)
        {
            GameObject GO = Instantiate(Ref.PF_radialItem, Ref.RT_radialHolder);
            RectTransform RT = GO.GetComponent<RectTransform>();
            Ref.rt_radialItems[i] = RT;
            RT.anchoredPosition = Vector2.zero;
            RT.localEulerAngles = new Vector3(0, 0, _rot);
            RT.GetChild(0).eulerAngles = Vector3.zero;
            _rot += _rotIncrease;
        }

        int _amt = 0;
        foreach (var item in _childAmt)
        {
            Ref.rt_radialItems_Sub.Add(new RectTransform[item]);
            Setup_SubGroup(_amt, item);
            _amt++;
        }
    }

    public void Setup_Guns(GunClass _equippedGun, GunClass[] _gunList)
    {
        Ref.rt_radialItems[0].GetChild(0).GetComponent<Image>().sprite = _equippedGun.sprite;
        for (int i = 0; i < Mathf.Min(_gunList.Length, Ref.rt_radialItems_Sub[0].Length); i++)
        {
            Ref.rt_radialItems_Sub[0][i].GetChild(0).GetComponent<Image>().sprite = _gunList[i].sprite;
        }
    }
    public void Setup_Consumables(List<Consumable.save> _consumables)
    {
        int _first = 999;
        Color _trans = new Color(1, 1, 1, 0.3f);

        for (int i = 0; i < Mathf.Min(_consumables.Count, Ref.rt_radialItems_Sub[1].Length); i++)
        {
            Ref.rt_radialItems_Sub[1][i].GetChild(0).GetComponent<Image>().sprite = _consumables[i].Get_Item().sprite;
            Ref.rt_radialItems_Sub[1][i].GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = _consumables[i]._amt.ToString();

            if (_consumables[i]._amt > 0)
            {
                Ref.rt_radialItems_Sub[1][i].GetChild(0).GetComponent<Image>().color = Color.white;
                Ref.rt_radialItems_Sub[1][i].GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
                _first = Mathf.Min(i, _first);
            }
            else
            {
                Ref.rt_radialItems_Sub[1][i].GetChild(0).GetComponent<Image>().color = _trans;
                Ref.rt_radialItems_Sub[1][i].GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().color = _trans;
            }
        }
        if (_first < _consumables.Count)
        {
            Ref.rt_radialItems[1].GetChild(0).GetComponent<Image>().sprite = _consumables[_first].Get_Item().sprite;
            Ref.rt_radialItems[1].GetChild(0).GetComponent<Image>().color = Color.white;
            Ref.rt_radialItems[1].GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
        }
        else if (_consumables.Count > 0)
        {
            Ref.rt_radialItems[1].GetChild(0).GetComponent<Image>().sprite = _consumables[0].Get_Item().sprite;
            Ref.rt_radialItems[1].GetChild(0).GetComponent<Image>().color = _trans;
            Ref.rt_radialItems[1].GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().color = _trans;
        }

        if (Ref.rt_radialItems_Sub[1].Length == 1)
            Ref.rt_radialItems[1].GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = _consumables[_first]._amt.ToString();
        else
            Ref.rt_radialItems[1].GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
    }

    public void Setup_SubGroup(int _subGroup, int _childAmt)
    {
        float _rot = (360 / Values.i_childAmt) * _subGroup;
        _rot -= Values.F_subGroupRange / 2;
        float _rotIncrease = Values.F_subGroupRange / (_childAmt - 1);
        for (int i = 0; i < _childAmt; i++)
        {
            GameObject GO = Instantiate(Ref.PF_radialItem, Ref.RT_radialHolder_Sub);
            RectTransform RT = GO.GetComponent<RectTransform>();
            Ref.rt_radialItems_Sub[_subGroup][i] = RT;
            RT.anchoredPosition = Vector2.zero;
            RT.localEulerAngles = new Vector3(0, 0, _rot);
            RT.GetChild(0).eulerAngles = Vector3.zero;
            RT.localScale = Vector3.one * 2.5f;
            _rot += _rotIncrease;
        }
    }

    void ClearChildren()
    {
        foreach (var item in Ref.rt_radialItems_Sub)
        {
            for (int i = item.Length - 1; i >= 0; i--)
                Destroy(item[i].gameObject);
        }
        Ref.rt_radialItems_Sub.Clear();

        for (int i = Ref.rt_radialItems.Length - 1; i >= 0; i--)
            Destroy(Ref.rt_radialItems[i].gameObject);
        Ref.rt_radialItems = new RectTransform[0];
        Values.i_childAmt = 0;
    }

    public void Show()
    {
        Ref.G_radialMenu.SetActive(true);
        ResetCursor();
    }
    public void Hide()
    {
        Ref.G_radialMenu.SetActive(false);
    }
    public void ResetCursor()
    {
        Ref.RT_cursor.anchoredPosition = Vector2.zero;
        ItemInfoRef.RT_holder.gameObject.SetActive(false);
        UpdateSelected();
    }
    public void MoveCursor(Vector2 _pos)
    {
        _pos *= Values.F_sensitivity * Time.unscaledDeltaTime;
        Ref.RT_cursor.anchoredPosition = Vector2.ClampMagnitude(Ref.RT_cursor.anchoredPosition + _pos, Values.F_radial_BaseDistance);
        Ref.LR_cursor.SetPosition(Ref.RT_cursor.anchoredPosition);
        UpdateSelected();
    }
    public void MoveCursor_Gamepad(Vector2 _pos)
    {
        _pos = Vector2.ClampMagnitude(_pos, 1);
        Ref.RT_cursor.anchoredPosition = _pos * Values.F_radial_BaseDistance;
        Ref.LR_cursor.SetPosition(Ref.RT_cursor.anchoredPosition);
        UpdateSelected();
    }

    void Set_selChild(int sel)
    {
        if (sel != Values.i_selChild)
        {
            Values.i_selChild = sel;
            PlayerController.Instance.AH_agentAudioHolder.Play(AgentAudioHolder.type.radialTick);
            Ref.G_radialSubBG.transform.localEulerAngles = new Vector3(0, 0, 4.5f + (((float)sel / (float)Values.i_childAmt) * 360f));
            Ref.RT_radialHolder_Sub.anchoredPosition = Quaternion.Euler(0, 0, (float)Values.i_selChild / (float)Values.i_childAmt * 360f) * Vector3.up * 400;
            ItemInfoRef.RT_holder.gameObject.SetActive(sel != -1);
            Set_selSubChild(-1);
        }
    }
    void Set_selSubChild(int sel)
    {
        if (sel != Values.i_selSubChild)
        {
            Values.i_selSubChild = sel;
            PlayerController.Instance.AH_agentAudioHolder.Play(AgentAudioHolder.type.radialSubTick);

            ItemInfoRef.RT_holder.gameObject.SetActive(sel != -1);
            if (sel >= 0)
            {
                //Coyote Time
                Values.i_lastChild = Values.i_selChild;
                Values.i_lastSubChild = Values.i_selSubChild;
                if (Values.C_CoyoteTime != null) StopCoroutine(Values.C_CoyoteTime);
                Values.C_CoyoteTime = StartCoroutine(CoyoteTime(Values.F_coyoteTime));
                //Display Info
                if (Values.i_selChild == 0)
                    ItemInfoRef.Display(PlayerController.Instance.gun_EquippedList[sel]);
                else if (Values.i_selChild == 1)
                    ItemInfoRef.Display(SaveData.consumables[sel]);
            }
        }
    }
    IEnumerator CoyoteTime(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        Values.i_lastChild = -1;
        Values.i_lastSubChild = -1;
    }

    void UpdateSelected()
    {
        Vector2 pos = Ref.RT_cursor.anchoredPosition;
        if (pos.magnitude > Values.F_deadzone)
        {
            float rot = Vector2.SignedAngle(Vector2.up, pos) / 360;
            if (rot < 0) rot += 1;

            if (Values.i_selChild == -1)
            {
                UpdateSelected_Base(rot);
            }
            else
            {
                if (Ref.rt_radialItems_Sub[Values.i_selChild].Length > 1)
                {
                    Ref.G_radialSubBG.SetActive(true);
                    UpdateSelected_Sub(rot);
                }
                else
                {
                    Ref.G_radialSubBG.SetActive(false);
                    Set_selSubChild(0);
                    UpdateSelected_Base(rot);
                }
            }
        }
        else
        {
            Ref.G_radialSubBG.SetActive(false);
            Set_selChild(-1);
        }
        DisplaySelected();
    }

    void UpdateSelected_Base(float rot)
    {
        rot *= Values.i_childAmt;
        float var = Mathf.Abs((rot % 1) - 0.5f);
        if (var >= Values.F_itemSelectionGapPercent)
        {
            int sel = Mathf.RoundToInt(rot);
            if (sel == Values.i_childAmt) sel = 0;
            Set_selChild(sel);
        }
    }
    void UpdateSelected_SubOLD(float rot)
    {
        float _rot = (1f / Values.i_childAmt) * Values.i_selChild;
        rot -= _rot;

        float _subGroupDivisor = 360f / Values.F_subGroupRange;
        rot += 1f / (_subGroupDivisor * 2f);

        if (rot < 0) rot += 1;
        if (rot > 1) rot -= 1;

        rot *= (Ref.rt_radialItems_Sub[Values.i_selChild].Length * _subGroupDivisor) - 1;
        float var = Mathf.Abs((rot % 1) - 0.5f);
        if (var >= Values.F_itemSelectionGapPercent)
        {
            int sel = Mathf.RoundToInt(rot);
            if (sel == Ref.rt_radialItems_Sub[Values.i_selChild].Length * _subGroupDivisor) Values.i_selSubChild = 0;
            else if (sel < Ref.rt_radialItems_Sub[Values.i_selChild].Length)
                Set_selSubChild(sel);
            else if (sel == Ref.rt_radialItems_Sub[Values.i_selChild].Length)
                Set_selSubChild(Ref.rt_radialItems_Sub[Values.i_selChild].Length - 1);
            else if (sel < (Ref.rt_radialItems_Sub[Values.i_selChild].Length * _subGroupDivisor) - 1)
                Set_selSubChild(0);
            else
            {
                Set_selSubChild(-1);
            }
        }
        else
            Set_selSubChild(-1);
    }
    void UpdateSelected_Sub(float rot)
    {
        //Adjust Rotation for SubGroup Arc
        float offset = 1f + (0.25f - ((float)Values.i_selChild / (float)Values.i_childAmt));
        rot = Mathf.Abs((rot + offset) % 1);

        //Multiply Rotation by List Count + leeway for none selected
        int count = (Ref.rt_radialItems_Sub[Values.i_selChild].Length * 2) - 1;
        rot *= count;

        //Round to Selection Int + adjust for start/end of active space
        int sel = Mathf.RoundToInt(rot);
        if (sel == count) sel = 0;
        if (sel == Ref.rt_radialItems_Sub[Values.i_selChild].Length) sel -= 1;

        //Select Selection
        if (sel > Ref.rt_radialItems_Sub[Values.i_selChild].Length)
            Set_selSubChild(-1);
        else
        {
            //Check if selected between options
            float var = Mathf.Abs((rot % 1) - 0.5f);
            if (var >= Values.F_itemSelectionGapPercent)
                Set_selSubChild(sel);
        }
    }
    void DisplaySelected()
    {
        for (int i = 0; i < Ref.rt_radialItems.Length; i++)
        {
            if (i == Values.i_selChild)
            {
                Ref.rt_radialItems[i].localScale = Vector3.Lerp(Ref.rt_radialItems[i].localScale, Vector3.one * 1.5f, Time.unscaledDeltaTime * 10);
                if (Ref.rt_radialItems_Sub[i].Length > 1)
                { 
                for (int j = 0; j < Ref.rt_radialItems_Sub[i].Length; j++)
                {
                    Ref.rt_radialItems_Sub[i][j].gameObject.SetActive(true);
                    if (j == Values.i_selSubChild) Ref.rt_radialItems_Sub[i][j].localScale = Vector3.Lerp(Ref.rt_radialItems_Sub[i][j].localScale, Vector3.one * 1.5f, Time.unscaledDeltaTime * 10);
                    else Ref.rt_radialItems_Sub[i][j].localScale = Vector3.Lerp(Ref.rt_radialItems_Sub[i][j].localScale, Vector3.one, Time.unscaledDeltaTime * 10);
                }
                }
            }
            else
            {
                Ref.rt_radialItems[i].localScale = Vector3.Lerp(Ref.rt_radialItems[i].localScale, Vector3.one, Time.unscaledDeltaTime * 10);
                foreach (var item in Ref.rt_radialItems_Sub[i])
                    item.gameObject.SetActive(false);
            }
        }
        MovePivot();
    }

    //Move Pivot to Sub Group
    void MovePivot()
    {
        //Check if Subgroup with children is selected
        bool _move = Values.i_selChild != -1;
        if (_move)
            _move = Ref.rt_radialItems_Sub[Values.i_selChild].Length > 1;
        //Move if so
        if (_move)
            Ref.RT_radialHolder.anchoredPosition = Vector3.Lerp(Ref.RT_radialHolder.anchoredPosition, Quaternion.Euler(0, 0, (float)Values.i_selChild / (float)Values.i_childAmt * 360f) * -Vector3.up * 400, Time.unscaledDeltaTime * 10);
        else
            Ref.RT_radialHolder.anchoredPosition = Vector3.Lerp(Ref.RT_radialHolder.anchoredPosition, Vector3.zero, Time.unscaledDeltaTime * 10);
    }
    public void Confirm()
    {
        if (Values.i_lastChild == 0)
        {
            if (Values.i_lastSubChild >= 0)
                PlayerController.Instance.Gun_Equip(Values.i_lastSubChild);
        }
        else if (Values.i_lastChild == 1)
        {
            if (Values.i_lastSubChild >= 0)
                PlayerController.Instance.Consumable_Use(SaveData.consumables[Values.i_lastSubChild].Get_Item());
        }
        Values.i_lastChild = -1;
        Values.i_lastSubChild = -1;
    }
}
