using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Animations.Rigging;

public static class StaticCoroutines
{
    public static IEnumerator Fade(this CanvasGroup _canvas, bool _fadeIn = true, float _speed = 0.1f)
    {
        if (_fadeIn || _canvas.gameObject.activeSelf)
        {
            _canvas.gameObject.SetActive(true);
            float _timer = 0f;
            float _start = _fadeIn ? 0f : 1f;
            float _end = _fadeIn ? 1f : 0f;
            while (_timer < 1f)
            {
                _canvas.alpha = Mathf.Lerp(_start, _end, _timer);
                yield return new WaitForEndOfFrame();
                _timer += Time.unscaledDeltaTime / _speed;
            }
            _canvas.alpha = _end;
            if (!_fadeIn)
                _canvas.gameObject.SetActive(false);
        }
    }
    public static IEnumerator Fade(this Volume _volume, bool _fadeIn = true, float _speed = 0.1f)
    {
        if (_fadeIn || _volume.gameObject.activeSelf)
        {
            _volume.gameObject.SetActive(true);
            float _timer = 0f;
            float _start = _fadeIn ? 0f : 1f;
            float _end = _fadeIn ? 1f : 0f;
            while (_timer < 1f)
            {
                _volume.weight = Mathf.Lerp(_start, _end, _timer);
                yield return new WaitForEndOfFrame();
                _timer += Time.unscaledDeltaTime / _speed;
            }
            _volume.weight = _end;
            if (!_fadeIn)
                _volume.gameObject.SetActive(false);
        }
    }
    public static IEnumerator Fade(this Rig _rig, bool _fadeIn = true, float _speed = 0.1f)
    {
        float _timer = 0f;
        float _start = _fadeIn ? 0f : 1f;
        float _end = _fadeIn ? 1f : 0f;
        while (_timer < 1f)
        {
            _rig.weight = Mathf.Lerp(_start, _end, _timer);
            yield return new WaitForEndOfFrame();
            _timer += Time.unscaledDeltaTime / _speed;
        }
        _rig.weight = _end;
    }

    public static IEnumerator Move(this Transform _target, Vector3 _tarPos, Quaternion _tarRot, bool _local, float _speed = 0.4f, AnimCurve _anim = null, UnityAction _action = null)
    {
        float _timer = 0f;
        Vector3 startPos;
        Quaternion startRot;
        float _progress = 0;
        if (_local)
        { startPos = _target.localPosition; startRot = _target.localRotation; }
        else
        { startPos = _target.position; startRot = _target.rotation; }

        while (_timer < 1)
        {
            if (_anim != null) _progress = _anim.Evaluate(_timer);
            else _progress = _timer;

            if (_local)
            {
                _target.localPosition = Vector3.Lerp(startPos, _tarPos, _progress);
                _target.localRotation = Quaternion.Lerp(startRot, _tarRot, _progress);
            }
            else
            {
                _target.position = Vector3.Lerp(startPos, _tarPos, _progress);
                _target.rotation = Quaternion.Lerp(startRot, _tarRot, _progress);
            }
            yield return new WaitForEndOfFrame();
            _timer += Time.unscaledDeltaTime / _speed;
        }
        if (_local)
        {
            _target.localPosition = _tarPos;
            _target.localRotation = _tarRot;
        }
        else
        {
            _target.position = _tarPos;
            _target.rotation = _tarRot;
        }
        if (_action != null) _action.Invoke();
    }
}
