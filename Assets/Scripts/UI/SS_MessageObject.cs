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

    private Vehicle v_vehicle;

    private Vector3 v3_offset = Vector3.zero;

    [System.Serializable]
    public class messageObjectClass
    {
        public GameObject G_holder;
        public Image[] I_image;
        public TextMeshProUGUI TM_distanceText;
        public TextMeshProUGUI TM_name;

        public GameObject G_healthHolder;
        public Image I_healthSlider;

        public RectTransform RT_hitPoint;

        public void Show(bool _show)
        {
            if (!G_holder)
                return;
            G_holder.SetActive(_show);
        }

        public void Setup(Sprite _sprite, string _name = "", bool _showDistance = false, Vehicle _vehicle = null)
        {
            if (!G_holder)
                return;
            foreach (var item in I_image)
                item.sprite = _sprite;

            TM_distanceText.gameObject.SetActive(_showDistance);

            if (TM_name != null)
                TM_name.text = _name;

            UpdateHealth(_vehicle);
        }
        public void UpdateHealth(Vehicle _vehicle)
        {
            if (_vehicle == null)
            {
                if (G_healthHolder) G_healthHolder.SetActive(false);
                if (RT_hitPoint) RT_hitPoint.gameObject.SetActive(false);
            }
            else
            {
                if (G_healthHolder) G_healthHolder.SetActive(true);
                if (RT_hitPoint) RT_hitPoint.gameObject.SetActive(true);

                if (I_healthSlider) I_healthSlider.fillAmount = _vehicle.f_curHealth / (float)_vehicle.I_maxHealth;
            }
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
        if (rect.FollowObject(c_canvas, t_target.position + v3_offset) && offscreenObject.G_holder != null)
        {
            mainObject.Show(false);
            offscreenObject.Show(true);
            if (showDistance)
                Update_DistanceText(offscreenObject);
            UpdateHitPoint(offscreenObject.RT_hitPoint);
        }
        else
        {
            mainObject.Show(true);
            offscreenObject.Show(false);
            if (showDistance)
                Update_DistanceText(mainObject);
            UpdateHitPoint(mainObject.RT_hitPoint);
        }
    }
    void UpdateHitPoint(RectTransform _rect)
    {
        if (_rect == null)
            return;
        if (v_vehicle == null)
            return;
        Vector3 _target = t_target.position;
        Vector3 _distTravelled = v_vehicle.GetVelocity();
        _distTravelled *= (Vector3.Distance(_target, t_player.position) / v_vehicle.GetWeaponSpeed());
        _target += _distTravelled;
        _rect.FollowObject(c_canvas, _target);
        _rect.anchoredPosition -= rect.anchoredPosition;
    }
    public void UpdateHealth()
    {
        mainObject.UpdateHealth(v_vehicle);
        offscreenObject.UpdateHealth(v_vehicle);
    }
    void Update_DistanceText(messageObjectClass _message)
    {
        float distance = Vector3.Distance(t_player.position, t_target.position);
        _message.TM_distanceText.text = distance.ToString_Distance();
    }

    public void Setup(Sprite _sprite, Transform _target, Canvas _canvas, string _name = "", bool _showDistance = false, Vehicle _vehicle = null)
    {
        showDistance = _showDistance;

        rect = GetComponent<RectTransform>();
        c_canvas = _canvas;
        t_target = _target;
        t_player = PlayerManager.main.RB.transform;
        v_vehicle = _vehicle;

        mainObject.Setup(_sprite, _name, _showDistance, _vehicle);
        offscreenObject.Setup(_sprite, _name, _showDistance, _vehicle);
    }

    public void Setup(Transform _target, Canvas _canvas, Vector3? _offset = null)
    {
        if (_offset != null)
            v3_offset = _offset.Value;

        showDistance = false;

        rect = GetComponent<RectTransform>();
        c_canvas = _canvas;
        t_target = _target;
        t_player = PlayerManager.main.RB.transform;
        v_vehicle = null;
    }
}
