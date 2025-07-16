using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class Button_MainMenu : MonoBehaviour, ISelectHandler// required interface when using the OnSelect method.
{
    //Do this when the selectable UI object is selected.
    public void OnSelect(BaseEventData eventData)
    {
        Camera_MainMenu.Instance.OnSelected(eventData);
    }
    public void OnPointerEnter(BaseEventData eventData)
    {
        Camera_MainMenu.Instance.OnSelected(eventData);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Camera_MainMenu.Instance.OnClicked(eventData);
    }
    public void OnSubmit(PointerEventData eventData)
    {
        Camera_MainMenu.Instance.OnClicked(eventData);
    }
}