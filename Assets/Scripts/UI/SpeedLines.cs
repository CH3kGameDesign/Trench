using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpeedLines : MonoBehaviour
{
    public Image image;
    private Coroutine coroutine;
    private string sMaskScale = "_MaskScale";
    private float tarAmt = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetMaskActive(false, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMaskActive(bool _on, float speed = 0.5f)
    {
        SetMaskScale(_on ? 0.1f : 0.5f, speed);
    }

    public void SetMaskScale(float amt, float speed = 0.5f)
    {
        if (tarAmt != amt)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = null;
            if (speed > 0f)
                coroutine = StartCoroutine(SetMask(amt, speed));
            else
                image.material.SetFloat(sMaskScale, amt);
            tarAmt = amt;
        }
    }

    IEnumerator SetMask(float _tar, float _speed)
    {
        float _timer = 0;
        float _start = image.material.GetFloat(sMaskScale);
        while (_timer < 1)
        {
            image.material.SetFloat(sMaskScale, Mathf.Lerp(_start, _tar, _timer));
            yield return new WaitForEndOfFrame();
            _timer += Time.deltaTime / _speed;
        }
        image.material.SetFloat(sMaskScale, _tar);
    }
}
