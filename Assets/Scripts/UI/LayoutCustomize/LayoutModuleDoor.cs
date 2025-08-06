using UnityEngine;
using UnityEngine.UI;

public class LayoutModuleDoor : MonoBehaviour
{
    [HideInInspector] public Vector2Int V2_localPos;
    [HideInInspector] public int I_localRot;
    [HideInInspector] public LayoutModuleObject LMO_Holder;
    [HideInInspector] public LayoutModuleDoor LMD_Connection;
    public Image I_BG;
    public RectTransform RT;
    public Color C_connected;
    public Color C_disconnected;

    public void Setup(LevelGen_Block.doorClass _door, LayoutModuleObject _LMO)
    {
        V2_localPos = _door._pos;
        I_localRot = _door._rot;
        LMO_Holder = _LMO;
        SetLocalTransform();
        SetConnected(null);
    }

    void SetLocalTransform()
    {
        RT.anchoredPosition = ((Vector2)V2_localPos - ((Vector2)LMO_Holder.Block.size/2) + new Vector2(0.5f,0.5f)) * 100;
        RT.localEulerAngles = new Vector3(0, 0, -90 * I_localRot);
    }

    public void SetConnected(LayoutModuleDoor _LMD)
    {
        LayoutModuleDoor _old = LMD_Connection;
        LMD_Connection = _LMD;

        if (LMD_Connection != null)
        {
            I_BG.color = C_connected;
        }
        else
        {
            I_BG.color = C_disconnected;
        }

        if (_old != null) 
            if (_old.LMD_Connection == this)
                _old.SetConnected(null);
    }

    public void GetWorldPos(out Vector2Int _pos, out int  _rot)
    {
        int _parentRot = LMO_Holder.I_rot;
        switch (_parentRot)
        {
            case 0: _pos = new Vector2Int(LMO_Holder.maxPos.x - V2_localPos.y, LMO_Holder.minPos.y + V2_localPos.x); break;
            case 1: _pos = new Vector2Int(LMO_Holder.maxPos.x - V2_localPos.x, LMO_Holder.maxPos.y - V2_localPos.y); break;
            case 2: _pos = new Vector2Int(LMO_Holder.minPos.x + V2_localPos.y, LMO_Holder.maxPos.y - V2_localPos.x); break;
            case 3: _pos = new Vector2Int(LMO_Holder.minPos.x + V2_localPos.x, LMO_Holder.minPos.y + V2_localPos.y); break;

            default:
                Debug.LogError("Rotation Out Of Range: " + _parentRot);
                _pos = LMO_Holder.minPos;
                break;
        }
        _rot = (_parentRot - I_localRot) % 4;
        if (_rot < 0) _rot += 4;
    }
}
