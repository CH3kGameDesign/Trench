using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    public TextMeshProUGUI TM_nameText;

    public RawImage S_icon;

    public Image S_healthSlider;
    public TextMeshProUGUI TM_healthText;
    float _health;
    Coroutine _healthUpdate = null;

    private Material _mat = null;

    [HideInInspector]public BaseController _controller;

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
        UpdateMaterialRef();
        _controller = _con;
        TM_nameText.text = _con.GetName();
        S_icon.texture = _con.T_icon;
        UpdateHealth(true);
    }

    void UpdateMaterialRef()
    {
        if (_mat == null)
        {
            _mat = new Material(S_healthSlider.material);
            S_healthSlider.material = _mat;
        }
    }

    public void UpdateHealth(bool _instant = false)
    {
        if (_healthUpdate != null)
            StopCoroutine(_healthUpdate);

        if (_instant)
        {
            _health = _controller.info.F_curHealth;
            _mat.SetFloat("_Value", _health / _controller.info.F_maxHealth);
            TM_healthText.text = Mathf.RoundToInt(_controller.info.F_curHealth).ToString();
        }
        else
            _healthUpdate = StartCoroutine(Health_Update(_controller.info.F_curHealth));
    }
    IEnumerator Health_Update(float _tarHealth)
    {
        float _timer = 0;
        float _oldHealth = _health;
        while (_timer < 1)
        {
            _health = Mathf.Lerp(_oldHealth, _tarHealth, _timer);
            _mat.SetFloat("_Value", _health / _controller.info.F_maxHealth);
            TM_healthText.text = Mathf.RoundToInt(_health).ToString();
            _timer += Time.deltaTime / 0.2f;
            yield return new WaitForEndOfFrame();
        }
        _mat.SetFloat("_Value", _tarHealth / _controller.info.F_maxHealth);
        TM_healthText.text = Mathf.RoundToInt(_tarHealth).ToString();
    }
}
