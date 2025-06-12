using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Decal_Handler : MonoBehaviour
{
    public GameObject parent;
    public DecalProjector projector;
    public ColorClass _Color;
    [System.Serializable]
    public class ColorClass
    {
        public bool _active = false;
        public Color color = Color.white;
        public Color color2 = Color.white;
        public bool _fadeOut = false;

        public void Activate(DecalProjector projector)
        {
            if (_active)
            {
                Material _mat = projector.material;
                Color _temp = Color.Lerp(color, color2, Random.Range(0f, 1f));
                _mat.SetColor("_Color", _temp);
            }
        }
    }
    public SizeClass _Size;
    [System.Serializable]
    public class SizeClass
    {
        public bool _active = false;
        public Vector2 sizeRange = Vector2.one;
        public bool _fadeOut = false;

    }
    private Vector3 _tarSize = Vector3.one;
    public float timeIncrease = 0.2f;
    public float lifeTime = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Material _mat = new Material(projector.material);
        projector.material = _mat;
        if (parent == null)
            parent = gameObject;
        _Color.Activate(projector);
        SizeChange();
        if (lifeTime > 0)
            Invoke(nameof(SizeDown), lifeTime);
    }

    void SizeChange()
    {
        if (_Size._active)
        {
            float _temp = Mathf.Lerp(_Size.sizeRange.x, _Size.sizeRange.y, Random.Range(0f, 1f));
            _tarSize = new Vector3(_temp, _temp, _tarSize.z);
        }
        else
            _tarSize = projector.size;

        if (timeIncrease > 0f)
            StartCoroutine(SizeChange(projector.size, _tarSize, timeIncrease));
        else
            projector.size = _tarSize;
    }

    void SizeDown()
    {
        if (gameObject.activeInHierarchy)
        {
            bool _timer = false;
            if (_Size._fadeOut)
            {
                StartCoroutine(SizeChange(projector.size, Vector3.zero, 1f));
                _timer = true;
            }
            if (_Color._fadeOut)
            {
                Material _mat = projector.material;
                Color _start = _mat.GetColor("_Color");
                Color _tar = _start;
                _tar.a = 0;
                StartCoroutine(ColorChange(_start, _tar, 1f));
                _timer = true;
            }
            if (_timer)
                Invoke(nameof(Destroy_Delayed), 1f);
        }
        else
            Destroy(parent);
    }
    void Destroy_Delayed()
    {
        Destroy(parent);
    }

    IEnumerator SizeChange(Vector3 _start, Vector3 _tar, float _speed)
    {
        float _timer = 0f;
        while (_timer < 1f)
        {
            projector.size = Vector3.Lerp(_start, _tar, _timer);
            yield return new WaitForEndOfFrame();
            _timer += Time.deltaTime / _speed;
        }
        projector.size = _tar;
    }

    IEnumerator ColorChange(Color _start, Color _tar, float _speed)
    {
        float _timer = 0f;
        Material _mat = projector.material;
        while (_timer < 1f)
        {
            Color _temp = Color.Lerp(_start, _tar, _timer);
            _mat.SetColor("_Color", _temp);
            yield return new WaitForEndOfFrame();
            _timer += Time.deltaTime / _speed;
        }
        _mat.SetColor("_Color", _tar);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
