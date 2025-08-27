using UnityEngine;

public class ShipLayout : MonoBehaviour
{
    public Layout_Bounds layoutBounds;

    private void OnDrawGizmos()
    {
        if (layoutBounds == null)
            return;
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Vector3 _size = new Vector3(5, 5, 5);
        Vector3 _pos = transform.position;
        int xCount = layoutBounds.data[0].d.Count;
        _pos.x -= ((float)xCount - 1) / 2f * _size.x;
        _pos.z -= ((float)layoutBounds.data.Count - 1) / 2f * _size.z;
        for (int y = 0; y < layoutBounds.data.Count; y++)
        {
            for (int x = 0; x < layoutBounds.data[y].d.Count; x++)
            {
                switch (layoutBounds.data[y].d[xCount - x - 1].roomtype)
                {
                    case Layout_Bounds.roomEnum.placeable:
                        Vector3 _offset = new Vector3(x * 5, 2.5f, y * 5);
                        Gizmos.DrawCube(_pos + _offset, _size);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
