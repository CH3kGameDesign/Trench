using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadialMenu_Child : MonoBehaviour
{
    public RectTransform RT;
    public Image I_BG;
    public Image I_sprite;
    public TextMeshProUGUI TM_amount;
    Color _trans = new Color(1, 1, 1, 0.3f);

    public Sprite S_active;
    public Sprite S_selected;
    public Sprite S_inactive;

    public void Setup(GunClass _gun, bool _main = false)
    {
        I_sprite.sprite = _gun.sprite;
        TM_amount.text = "";
        I_BG.sprite = _main ? S_selected : S_active;
    }

    public bool Setup(Consumable.save _item, bool _main = false)
    {
        I_sprite.sprite = _item.Get_Item().sprite;
        TM_amount.text = _item._amt.ToString();

        bool inStock = false;

        if (_item._amt > 0)
        {
            I_BG.sprite = _main ? S_selected : S_active;
            I_sprite.color = Color.white;
            TM_amount.color = Color.white;
            inStock = true;
        }
        else
        {
            I_BG.sprite = S_inactive;
            I_sprite.color = _trans;
            TM_amount.color = _trans;
        }
        return inStock;
    }
}
