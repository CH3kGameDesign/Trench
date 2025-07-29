using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SS_MessageObject : MonoBehaviour
{
    private RectTransform rect;

    public messageObjectClass mainObject;
    public messageObjectClass offscreenObject;

    private bool showDistance;
    private Canvas c_canvas;
    private Transform t_target;
    private Transform t_player;

    [System.Serializable]
    public class messageObjectClass
    {
        public GameObject G_holder;
        public Image[] I_image;
        public TextMeshProUGUI TM_distanceText;
        public TextMeshProUGUI TM_name;

        public void Show(bool _show)
        {
            if (!G_holder)
                return;
            G_holder.SetActive(_show);
        }

        public void Setup(Sprite _sprite, string _name = "", bool _showDistance = false)
        {
            if (!G_holder)
                return;
            foreach (var item in I_image)
                item.sprite = _sprite;

            TM_distanceText.gameObject.SetActive(_showDistance);

            if (TM_name != null)
                TM_name.text = _name;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (t_target == null)
            return;
        if (rect.FollowObject(c_canvas, t_target) && offscreenObject.G_holder != null)
        {
            mainObject.Show(false);
            offscreenObject.Show(true);
            if (showDistance)
                Update_DistanceText(offscreenObject);
        }
        else
        {
            mainObject.Show(true);
            offscreenObject.Show(false);
            if (showDistance)
                Update_DistanceText(mainObject);
        }
    }

    void Update_DistanceText(messageObjectClass _message)
    {
        float distance = Vector3.Distance(t_player.position, t_target.position);
        _message.TM_distanceText.text = distance.ToString_Distance();
    }

    public void Setup(Sprite _sprite, Transform _target, Canvas _canvas, string _name = "", bool _showDistance = false)
    {
        showDistance = _showDistance;

        rect = GetComponent<RectTransform>();
        c_canvas = _canvas;
        t_target = _target;
        t_player = PlayerManager.main.RB.transform;

        mainObject.Setup(_sprite, _name, _showDistance);
        offscreenObject.Setup(_sprite, _name, _showDistance);
    }
}
