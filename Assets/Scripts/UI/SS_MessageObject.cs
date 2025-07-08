using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(RectTransform))]
public class SS_MessageObject : MonoBehaviour
{
    private RectTransform rect;
    public Image[] I_image;
    public TextMeshProUGUI TM_distanceText;
    private bool showDistance;
    private Canvas c_canvas;
    private Transform t_target;
    private Transform t_player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rect.FollowObject(c_canvas, t_target);
        if (showDistance)
            Update_DistanceText();
    }

    void Update_DistanceText()
    {
        float distance = Vector3.Distance(t_player.position, t_target.position);
        TM_distanceText.text = distance.ToString_Distance();
    }

    public void Setup(Sprite _sprite, Transform _target, Canvas _canvas, bool _showDistance = false)
    {
        foreach (var item in I_image)
            item.sprite = _sprite;

        showDistance = _showDistance;
        TM_distanceText.gameObject.SetActive(showDistance);

        rect = GetComponent<RectTransform>();
        c_canvas = _canvas;
        t_target = _target;
        t_player = PlayerManager.main.RB.transform;
    }
}
