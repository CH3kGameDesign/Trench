using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Resource : MonoBehaviour
{
    public Image I_icon;
    public TextMeshProUGUI TM_text;

    public void SetResource(Resource.resourceType _resource, int _gain, int _total)
    {
        I_icon.sprite = _resource.image;
        string _temp = "<b>"+_resource._name +"</b>: +" + _gain + " [" + _total + "]";
        TM_text.text = _temp;
    }
    public void SetMoney(int _gain, int _total)
    {
        string _temp = "<b>Money</b>: +" + _gain + " [" + _total + "]";
        TM_text.text = _temp;
    }
}
