using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    [System.Serializable]
    public class RefClass
    {
        public GameObject G_radialMenu;
        public GameObject PF_radialItem;
        public RectTransform RT_radialHolder;
        public RectTransform RT_radialHolder_Sub;
        public RectTransform RT_cursor;
        [HideInInspector] public RectTransform[] rt_radialItems = new RectTransform[0];
        [HideInInspector] public List<RectTransform[]> rt_radialItems_Sub = new List<RectTransform[]>();
    }
    public RefClass Ref = new RefClass();

    [System.Serializable]
    public class ValueClass
    {
        [HideInInspector] public int i_childAmt = 0;
        [HideInInspector] public int i_selChild = -1;
        [HideInInspector] public int i_selSubChild = -1;

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
        UpdateSelected();
    }
    public void MoveCursor(Vector2 _pos)
    {
        _pos *= Values.F_sensitivity * Time.unscaledDeltaTime;
        Ref.RT_cursor.anchoredPosition = Vector2.ClampMagnitude(Ref.RT_cursor.anchoredPosition + _pos, Values.F_radial_BaseDistance);
        UpdateSelected();
    }
    public void MoveCursor_Gamepad(Vector2 _pos)
    {
        _pos = Vector2.ClampMagnitude(_pos, 1);
        Ref.RT_cursor.anchoredPosition = _pos * Values.F_radial_BaseDistance;
        UpdateSelected();
    }

    void Set_selChild(int sel)
    {
        if (sel != Values.i_selChild)
        {
            Values.i_selChild = sel;
            PlayerController.Instance.AH_agentAudioHolder.Play(AgentAudioHolder.type.radialTick);
        }
    }
    void Set_selSubChild(int sel)
    {
        if (sel != Values.i_selSubChild)
        {
            Values.i_selSubChild = sel;
            PlayerController.Instance.AH_agentAudioHolder.Play(AgentAudioHolder.type.radialSubTick);
        }
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
                rot *= Values.i_childAmt;
                float var = Mathf.Abs((rot % 1) - 0.5f);
                if (var >= Values.F_itemSelectionGapPercent)
                {
                    int sel = Mathf.RoundToInt(rot);
                    if (sel == Values.i_childAmt) sel = 0;
                    Set_selChild(sel);
                }
            }
            else
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
        }
        else Set_selChild(-1);
        DisplaySelected();
    }
    void DisplaySelected()
    {
        for (int i = 0; i < Ref.rt_radialItems.Length; i++)
        {
            if (i == Values.i_selChild)
            {
                Ref.rt_radialItems[i].localScale = Vector3.Lerp(Ref.rt_radialItems[i].localScale, Vector3.one * 1.5f, Time.unscaledDeltaTime * 10);
                for (int j = 0; j < Ref.rt_radialItems_Sub[i].Length; j++)
                {
                    Ref.rt_radialItems_Sub[i][j].gameObject.SetActive(true);
                    if (j == Values.i_selSubChild) Ref.rt_radialItems_Sub[i][j].localScale = Vector3.Lerp(Ref.rt_radialItems_Sub[i][j].localScale, Vector3.one * 1.5f, Time.unscaledDeltaTime * 10);
                    else Ref.rt_radialItems_Sub[i][j].localScale = Vector3.Lerp(Ref.rt_radialItems_Sub[i][j].localScale, Vector3.one, Time.unscaledDeltaTime * 10);
                }
            }
            else
            {
                Ref.rt_radialItems[i].localScale = Vector3.Lerp(Ref.rt_radialItems[i].localScale, Vector3.one, Time.unscaledDeltaTime * 10);
                foreach (var item in Ref.rt_radialItems_Sub[i])
                    item.gameObject.SetActive(false);
            }
        }

        if (Values.i_selChild == -1)
            Ref.RT_radialHolder.anchoredPosition = Vector3.Lerp(Ref.RT_radialHolder.anchoredPosition, Vector3.zero, Time.unscaledDeltaTime * 10);
        else
        {
            Ref.RT_radialHolder.anchoredPosition = Vector3.Lerp(Ref.RT_radialHolder.anchoredPosition, Quaternion.Euler(0,0, (float)Values.i_selChild /(float)Values.i_childAmt * 360f) * -Vector3.up * 400, Time.unscaledDeltaTime * 10);
        }
    }
    public void Confirm()
    {
        if (Values.i_selChild == 0)
        {
            if (Values.i_selSubChild >= 0)
                PlayerController.Instance.Gun_Equip(Values.i_selSubChild);
        }    
    }
}
