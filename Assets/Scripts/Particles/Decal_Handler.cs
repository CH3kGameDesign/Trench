using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Decal_Handler : MonoBehaviour
{
    public DecalProjector projector;
    public ColorClass _Color;
    [System.Serializable]
    public class ColorClass
    {
        public bool _active = false;
        public Color color = Color.white;
        public Color color2 = Color.white;

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
        
    }
    private Vector3 _tarSize = Vector3.one;
    public float timeIncrease = 0.2f;
    public float lifeTime = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            StartCoroutine(SizeChange(_tarSize, timeIncrease));
        else
            projector.size = _tarSize;
    }

    void SizeDown()
    {
        StartCoroutine(SizeChange(Vector3.zero, 1f));
    }

    IEnumerator SizeChange(Vector3 _tar, float _speed)
    {
        Vector3 _start = projector.size;
        float _timer = 0f;
        while (_timer < 1f)
        {
            projector.size = Vector3.Lerp(_start, _tar, _timer);
            yield return new WaitForEndOfFrame();
            _timer += Time.deltaTime / _speed;
        }
        projector.size = _tar;
        if (_tar == Vector3.zero)
            Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
