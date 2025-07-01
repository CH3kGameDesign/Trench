using UnityEngine;
using UnityEngine.UI;

public class LineRendererUI : MonoBehaviour
{
    [SerializeField] private RectTransform m_myTransform;
    [SerializeField] private Image m_image;
    [Space(10)]
    [SerializeField] private float f_size = 15f;
    [Space(10)]
    [SerializeField] private Vector2 V2_posOne = Vector2.zero;
    [SerializeField] private Vector2 V2_posTwo = Vector2.zero;

    private void Start()
    {
        CreateLine(V2_posOne, V2_posTwo);
    }
    public void CreateLine(Vector2 posOne, Vector2 posTwo)
    {
        V2_posOne = posOne;
        V2_posTwo = posTwo;
        Vector2 midpoint = (posOne + posTwo) / 2f;

        m_myTransform.anchoredPosition = midpoint;

        Vector2 dir = posOne - posTwo;
        m_myTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        m_myTransform.localScale = new Vector3(dir.magnitude, f_size, 1f);
    }
    public void CreateLine(Vector2 posOne, Vector2 posTwo, Color color)
    {
        m_image.color = color;
        CreateLine(posOne, posTwo);
    }

    public void SetPosition(Vector3 posTwo)
    {
        CreateLine(V2_posOne, posTwo);
    }
}