using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolBeltButton : MonoBehaviour
{
    public Button B_button;
    public Image I_sprite;
    public GameObject G_selected;
    public TextMeshProUGUI TM_name;

    private PointerBuilder PB;
    [HideInInspector] public PointerBuilder.BeltClass beltClass;
    [HideInInspector] public PointerBuilder.SubClass subClass;
    [HideInInspector] public PointerBuilder.SubItem item;
    [HideInInspector] public bool locked;
    [HideInInspector] public enumType _enum = enumType.belt;
    public enum enumType { belt, sub};

    public void Setup(PointerBuilder.BeltClass _beltClass, PointerBuilder.SubClass _subClass, PointerBuilder _pb)
    {
        _enum = enumType.belt;
        PB = _pb;
        beltClass = _beltClass;
        subClass = _subClass;
        I_sprite.sprite = subClass.image;
        TM_name.text = subClass.name;
    }
    public void Setup(PointerBuilder.SubClass _subClass, PointerBuilder.SubItem _item, PointerBuilder _pb)
    {
        _enum = enumType.sub;
        PB = _pb;
        subClass = _subClass;
        item = _item;
        if (item.image != null)
        {
            Sprite sprite = Sprite.Create(_item.image, new Rect(0, 0, _item.image.width, _item.image.height), new Vector2(_item.image.width / 2, _item.image.height / 2));
            I_sprite.sprite = sprite;
        }
        TM_name.text = _item.name;
    }

    public void OnPress()
    {
        switch (_enum)
        {
            case enumType.belt:
                PB.BeltButtonTap_Sub(beltClass, subClass);
                break;
            case enumType.sub:
                PB.BeltButtonTap_Full(subClass, item);
                break;
            default:
                break;
        }
        Selected(true);
    }

    public void Selected(bool _sel)
    {
        G_selected.SetActive(_sel);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
