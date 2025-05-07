using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Treasure_Point : MonoBehaviour
{
    public TextMeshProUGUI TM_valueText;
    public Transform T_sizeChanger;
    [HideInInspector] public int I_totalValue = 0;
    private List<Treasure> t_treasure = new List<Treasure>();

    public sizeClass sizeUp = new sizeClass();
    public sizeClass sizeDown = new sizeClass();

    private Color C_baseColor = Color.white;
    private Coroutine _sizeCoroutine;

    [System.Serializable]
    public class sizeClass
    {
        public float _sizeIncrease = 0.5f;
        public Color _color = new Color(0, 1, 0, 1);
        public float _duration = 0.5f;
        public AnimCurve _curve;
    }

    private void Start()
    {
        TM_valueText.text = I_totalValue.ToString_Currency();
        C_baseColor = TM_valueText.color;
    }

    public void UpdatePoints()
    {
        int _value = 0;
        foreach (var item in t_treasure)
            _value += item.I_value;
        if (_sizeCoroutine != null)
            StopCoroutine(_sizeCoroutine);
        _sizeCoroutine = StartCoroutine(C_ValueChange(_value));
    }

    private void OnTriggerEnter(Collider other)
    {
        Treasure _temp;
        if (other.TryGetComponent<Treasure>(out _temp))
        {
            if (!t_treasure.Contains(_temp))
            {
                t_treasure.Add(_temp);
                PlayerController.Instance.Update_Objectives(Objective_Type.Collect_Value, _temp.I_value);
                other.gameObject.SetActive(false);
                UpdatePoints();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Treasure _temp;
        if (other.TryGetComponent<Treasure>(out _temp))
        {
            if (t_treasure.Contains(_temp))
            {
                t_treasure.Remove(_temp);
                UpdatePoints();
            }
        }
    }

    private IEnumerator C_ValueChange(int _newValue)
    {
        int _prevValue = I_totalValue;
        int _curValue = _prevValue;
        int _distance = Mathf.Abs(_newValue - _prevValue);

        float _timer = 0;
        float _timerScaled = 0;
        float _progress = 0;

        float _sizeIncrease;
        float _duration;
        AnimCurve _curve;

        if (_distance > 0)
        {
            if (_newValue > _prevValue)
            {
                _sizeIncrease = sizeUp._sizeIncrease;
                TM_valueText.color = sizeUp._color;
                _duration = sizeUp._duration;
                _curve = sizeUp._curve;
            }
            else
            {
                _sizeIncrease = sizeDown._sizeIncrease;
                TM_valueText.color = sizeDown._color;
                _duration = sizeDown._duration;
                _curve = sizeDown._curve;
            }
            if (_duration != 0)
            {
                while (_timer < 1)
                {
                    _timerScaled = _curve.Evaluate(_timer);
                    _curValue = Mathf.RoundToInt(Mathf.Lerp(_prevValue, _newValue, _timerScaled));
                    TM_valueText.text = _curValue.ToString_Currency();
                    I_totalValue = _curValue;
                    _progress = Mathf.FloorToInt(_timerScaled * _distance) / (float)_distance;

                    T_sizeChanger.localScale = Vector3.one * (1 + Mathf.Lerp(0, _sizeIncrease, _progress));
                    yield return new WaitForEndOfFrame();
                    _timer += Time.deltaTime / _duration;
                }
            }
            TM_valueText.text = _newValue.ToString_Currency();
            I_totalValue = _newValue;
            _sizeCoroutine = StartCoroutine(C_StopChange());
        }
        else
        {
            TM_valueText.text = _newValue.ToString_Currency();
            I_totalValue = _newValue;
            TM_valueText.color = C_baseColor;
            T_sizeChanger.localScale = Vector3.one;
        }
    }
    private IEnumerator C_StopChange()
    {
        yield return new WaitForSeconds(0.25f);
        float _timer = 0;
        float _prevScale = T_sizeChanger.localScale.x - 1;
        float _duration = 0.2f;
        TM_valueText.color = C_baseColor;
        while (_timer < 1)
        {
            T_sizeChanger.localScale = Vector3.one * (1 + Mathf.Lerp(_prevScale, 0, _timer));
            yield return new WaitForEndOfFrame();
            _timer += Time.deltaTime / _duration;
        }
    }
}
