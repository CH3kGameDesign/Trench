using UnityEngine;
using UnityEngine.Rendering.Universal;


public class RendererUpdate : MonoBehaviour
{
    public UniversalRendererData URD;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Set();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDestroy()
    {
        Unset();
    }

    void Set()
    {
        foreach (var item in URD.rendererFeatures)
        {
            if (item is RenderObjects)
                item.SetActive(false);
        }
    }
    void Unset()
    {
        foreach (var item in URD.rendererFeatures)
        {
            item.SetActive(true);
        }
    }
}
