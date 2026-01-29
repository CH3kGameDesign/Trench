using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UI_ValueSlider : MonoBehaviour
{
    public Slider S_Slider;
    public TextMeshProUGUI TM_value;
    public Vector2 V2_scale = new Vector2(-1, 1);

    public UnityEvent<float> OnValueChanged_Event = new UnityEvent<float>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetValue(float _value, Vector2 _bounds)
    {
        float _temp = (_value - _bounds.x) / (_bounds.y - _bounds.x);
        SetValue(_temp);
    }
    public void SetValue(float _value, float _min, float _max)
    {
        float _temp = (_value - _min) / (_max - _min);
        SetValue(_temp);
    }
    public void SetValue(float _value)
    {
        if (S_Slider.value != _value)
            S_Slider.value = _value;
        float _string = Mathf.Lerp(V2_scale.x, V2_scale.y, _value);
        TM_value.text = _string.ToString("0.0");
    }
    public void OnValueChanged(float _value)
    {
        float _string = Mathf.Lerp(V2_scale.x, V2_scale.y, _value);
        TM_value.text = _string.ToString("0.0");
        OnValueChanged_Event.Invoke(_value);
    }
}
