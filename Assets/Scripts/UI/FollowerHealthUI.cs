using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FollowerHealthUI : MonoBehaviour
{
    public TextMeshProUGUI TM_nameText;

    public Slider S_healthSlider;
    public TextMeshProUGUI TM_healthText;

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
        _controller = _con;
        TM_nameText.text = _con.GetName();
        UpdateHealth(true);
    }

    public void UpdateHealth(bool _instant = false)
    {
        if (_instant)
        {
            S_healthSlider.maxValue = _controller.F_maxHealth;
            S_healthSlider.value = _controller.F_curHealth;
            TM_healthText.text = Mathf.RoundToInt(S_healthSlider.value).ToString();
        }
        else
            StartCoroutine(Health_Update(_controller.F_curHealth));
    }
    IEnumerator Health_Update(float _health)
    {
        float _timer = 0;
        float _oldHealth = S_healthSlider.value;
        S_healthSlider.maxValue = _controller.F_maxHealth;
        while (_timer < 1)
        {
            S_healthSlider.value = Mathf.Lerp(_oldHealth, _health, _timer);
            TM_healthText.text = Mathf.RoundToInt(S_healthSlider.value).ToString();
            _timer += Time.deltaTime / 0.2f;
            yield return new WaitForEndOfFrame();
        }
        S_healthSlider.value = _health;
        TM_healthText.text = Mathf.RoundToInt(S_healthSlider.value).ToString();
    }
}
