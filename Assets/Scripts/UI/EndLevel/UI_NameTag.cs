using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_NameTag : MonoBehaviour
{
    public TextMeshProUGUI TM_name;
    public TextMeshProUGUI TM_label;

    public RawImage RI_icon;
    public Image I_deathBG;

    public GameObject G_deathIcon;

    public void Setup(string _name, string _label = "", Texture _icon = null, bool _alive = true)
    {
        if (TM_name)
            TM_name.text = _name;
        if (TM_label)
            TM_label.text = _label;

        if (RI_icon)
        {
            if (_icon == null)
                RI_icon.gameObject.SetActive(false);
            else
            {
                RI_icon.gameObject.SetActive(true);
                RI_icon.texture = _icon;
            }
        }

        if (G_deathIcon)
            G_deathIcon.SetActive(_alive);
        if (I_deathBG)
            I_deathBG.color = _alive ?
            new Color32(0x23, 0x23, 0x23, 0xFF) :
            new Color32(0xCD, 0x5C, 0x5C, 0xFF);
    }
}
