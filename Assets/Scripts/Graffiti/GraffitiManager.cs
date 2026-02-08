using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class GraffitiManager : MonoBehaviour
{
    public static GraffitiManager Instance;
    [System.Serializable]
    public class layerClass
    {
        public string _stampID;
        public Vector2Int _position;
        public Vector2 _scale;
        public float _rotation;
        public Color _color;

        public layerClass()
        {
            _stampID = "";
        }
    }

    [System.Serializable]
    public class graffitiClass
    {
        public string _name;
        private Texture2D _texture = null;
        public List<layerClass> _layers = new List<layerClass>();

        public graffitiClass()
        {
            _name = "New Graffiti";
            _layers = new List<layerClass>(18);
        }

        public Texture2D GetTexture()
        {
            if (_texture == null)
                _texture = GraffitiManager.Instance.RenderGraffiti(this);
            return _texture;
        }
    }

    public enum stampTypeEnum { primitives, letters, complex}
    public Stamp_List stampList;
    [Space(10)]
    public Camera graffitiRenderer;
    public RectTransform RT_layers;
    public RenderTexture renderTex;
    [Space(10)]
    public UI_graffitiLayer PF_UI_graffitiLayer;
    public Image PF_image;

    public List<Stamp_Scriptable> GetList(stampTypeEnum _type)
    {
        List<Stamp_Scriptable> _list = new List<Stamp_Scriptable>();
        foreach (var item in stampList.list)
        {
            if (item.stampType == _type)
                _list.Add(item);
        }
        return _list;
    }

    private void Awake()
    {
        Instance = this;
    }

    public Stamp_Scriptable GetStamp(string _stampID)
    {
        foreach (var item in stampList.list)
        {
            if (item._stampID == _stampID)
                return item;
        }

        return null;
    }

    public Texture2D RenderGraffiti(graffitiClass _graffiti)
    {
        graffitiRenderer.gameObject.SetActive(true);

        for (int i = _graffiti._layers.Count - 1; i >= 0; i--)
        {
            if (_graffiti._layers[i]._stampID == null)
                continue;
            UI_graffitiLayer _temp = Instantiate(PF_UI_graffitiLayer);
            _temp.Setup(this);
            _temp.SetLayerInfo(_graffiti._layers[i]);
            Destroy(_temp.gameObject);
        }

        graffitiRenderer.Render();

        Texture2D tex = new Texture2D(renderTex.width, renderTex.height);
        RenderTexture.active = renderTex;
        tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        tex.Apply();

        //RT_layers.DeleteChildren();
        //graffitiRenderer.gameObject.SetActive(false);

        return tex;
    }
}
