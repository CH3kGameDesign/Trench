using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunUI : MonoBehaviour
{
    public TextMeshProUGUI TM_nameText;
    public TextMeshProUGUI TM_equipControl;

    public RawImage S_primaryIcon;
    public RawImage S_secondaryIcon;

    public TextMeshProUGUI TM_clipActive;
    public TextMeshProUGUI TM_clipSize;
    public TextMeshProUGUI TM_clipSecondary;

    [HideInInspector] public BaseController _controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Setup(BaseController _con)
    {
        _controller = _con;
    }
    public void Equip(GunClass _primary, GunClass _secondary)
    {
        TM_nameText.text = _primary._name;

        S_primaryIcon.texture = _primary.image;
        S_secondaryIcon.texture = _secondary.image;

        TM_clipActive.text = _primary.clipAmmo.ToString_Clip();
        TM_clipSize.text = _primary.clipVariables.clipSize.ToString_Clip();
        TM_clipSecondary.text = _secondary.clipAmmo.ToString_Clip();
    }
    public void UpdateClip(GunClass _primary)
    {
        TM_clipActive.text = _primary.clipAmmo.ToString_Clip();
    }
    public void UpdateControl()
    {
        TM_equipControl.text = "".ToString_Input(PlayerController.inputActions.RadialMenu, TM_equipControl, Interactable.enumType.input);
    }
}
