using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArmorOptionButton : MonoBehaviour
{
    public Button B_button;
    public Image I_sprite;
    public GameObject G_selected;
    public TextMeshProUGUI TM_name;

    public ArmorPiece armorClass;

    public bool locked;

    public void Setup(ArmorPiece _armor)
    {
        armorClass = _armor;
        Sprite sprite = Sprite.Create(_armor.image, new Rect(0, 0, _armor.image.width, _armor.image.height), new Vector2(_armor.image.width / 2, _armor.image.height / 2));
        I_sprite.sprite = sprite;
        TM_name.text = _armor.name;
    }

    public void OnPress()
    {
        MainMenu.Instance.ChooseArmor(armorClass);
        MainMenu.Instance.customize._equipped.Equipped(false);
        Equipped(true);
    }

    public void Equipped(bool _equipped)
    {
        if (_equipped)
            MainMenu.Instance.customize._equipped = this;
        G_selected.SetActive(_equipped);
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
