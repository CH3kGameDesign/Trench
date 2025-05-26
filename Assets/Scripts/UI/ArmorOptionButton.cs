using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArmorOptionButton : MonoBehaviour
{
    public Button B_button;
    public Image I_sprite;
    public GameObject G_selected;
    public TextMeshProUGUI TM_name;

    public ArmorManager.ArmorClass armorClass;

    public bool locked;

    public void Setup(ArmorManager.ArmorClass _armor)
    {
        armorClass = _armor;
        I_sprite.sprite = _armor.image;
        TM_name.text = _armor.name;
    }

    public void OnPress()
    {
        MainMenu.Instance.ChooseArmor(armorClass);
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
