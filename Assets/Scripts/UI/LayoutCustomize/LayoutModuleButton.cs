using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LayoutModuleButton : MonoBehaviour
{
    [HideInInspector] public LevelGen_Block Block;

    public TextMeshProUGUI TM_name;
    public TextMeshProUGUI TM_size;
    public Image I_icon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(LevelGen_Block _block)
    {
        TM_name.text = _block._name;
        SizeText_Update(_block.size);
        I_icon.sprite = _block.icon;
        Block = _block;
    }

    void SizeText_Update(Vector2Int _size)
    {
        string _temp = "";
        for (int y = 0; y < _size.y; y++)
        {
            for (int x = 0; x < _size.x; x++)
                _temp += ".";
            if (y <  _size.y - 1)
                _temp += "\n";
        }
        TM_size.text = _temp;
    }

    public void OnClick()
    {
        LayoutCustomize.Instance.SelectBuild(Block);
    }
}
