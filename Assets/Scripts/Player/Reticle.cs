using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Reticle : MonoBehaviour
{
    public GameObject PF_hitMarker;
    public GameObject PF_explosionMarker;
    public GameObject PF_killMarker;
    [Space(10)]
    public Image I_Reticle;
    public Sprite S_aimReticle;
    public TextMeshProUGUI[] TM_roundCounter;

    Coroutine c_rotate;

    public void UpdateRoundCount(GunClass _gun)
    {
        UpdateRoundCount(_gun.clipAmmo, _gun.clipVariables.clipSize);
    }

    public void Hit(HitObject.damageTypeEnum _type = HitObject.damageTypeEnum.bullet)
    {
        GameObject GO;
        switch (_type)
        {
            case HitObject.damageTypeEnum.explosive:
                GO = Instantiate(PF_explosionMarker, I_Reticle.transform);
                break;
            default:
                GO = Instantiate(PF_hitMarker, I_Reticle.transform);
                break;
        }
        GO.transform.localPosition = Vector3.zero;
        Destroy(GO, 1);
    }

    public void Kill()
    {
        GameObject GO;
        GO = Instantiate(PF_killMarker, I_Reticle.transform);
        GO.transform.localPosition = Vector3.zero;
        Destroy(GO, 1);
    }

    public void UpdateRoundCount(int _remaining, int _total)
    {
        string _temp = "";
        for (int i = 0; i < _remaining; i++)
            _temp += "I";
        _temp += "<color=#29292929>";
        int _used = _total - _remaining;
        for (int i = 0; i < _used; i++)
            _temp += "I";

        foreach (var item in TM_roundCounter)
            item.text = _temp;

        _temp += "</color>";
    }

    public void RotateReticle(float _duration)
    {
        if (c_rotate != null)
            StopCoroutine(c_rotate);
        c_rotate = StartCoroutine(RotateReticle_Coroutine(_duration, new Vector3(0, 0, -45),null));
    }

    public void ReloadReticle(float _duration,GunClass _gun)
    {
        if (c_rotate != null)
            StopCoroutine(c_rotate);
        c_rotate = StartCoroutine(RotateReticle_Coroutine(_duration, new Vector3(0, 0, 45),_gun));
    }

    IEnumerator RotateReticle_Coroutine(float _duration, Vector3 _rot, GunClass _gun)
    {
        //I_Reticle.rectTransform.eulerAngles = new Vector3(0, 0, -45);
        I_Reticle.rectTransform.localScale = Vector3.one;
        //Quaternion _quat = I_Reticle.rectTransform.localRotation;
        float _timer = 0;
        while (_timer < 1)
        {
            I_Reticle.rectTransform.localEulerAngles = Vector3.Lerp(_rot, Vector3.zero, _timer);
            _timer += Time.deltaTime / (_duration * 0.9f);
            yield return new WaitForEndOfFrame();
        }
        I_Reticle.rectTransform.localEulerAngles = Vector3.zero;
        _timer = 0;
        while (_timer < 1)
        {
            I_Reticle.rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.1f, _timer);
            _timer += Time.deltaTime / (_duration * 0.05f);
            yield return new WaitForEndOfFrame();
        }
        _timer = 0;
        while (_timer < 1)
        {
            I_Reticle.rectTransform.localScale = Vector3.Lerp(Vector3.one * 1.1f, Vector3.one, _timer);
            _timer += Time.deltaTime / (_duration * 0.05f);
            yield return new WaitForEndOfFrame();
        }
        I_Reticle.rectTransform.localScale = Vector3.one;
        if (_gun != null)
            UpdateRoundCount(_gun);
    }
}
