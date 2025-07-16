using System.Collections;
using UnityEngine;

public class Camera_MainMenu : MonoBehaviour
{
    public static Camera_MainMenu Instance;

    public Transform T_camHolder;
    public Transform T_camera;
    public AnimCurve AC_moveCurve;
    [Space(10)]
    public pointClass Main;
    public pointClass Lobby;
    public pointClass Search;
    [System.Serializable]
    public class pointClass
    {
        public Transform T_point;

        [Range(0,0.5f)]public float F_moveDur = 0.2f;
    }

    public enum points { main, lobby, search};

    private Coroutine C_move = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        MoveTo(Main);
    }

    // Update is called once per frame
    void Update()
    {
        Main.T_point.localEulerAngles += new Vector3(0, Time.deltaTime, 0);
    }

    public void MoveTo(points _point)
    {
        switch (_point)
        {
            case points.main: MoveTo(Main); break;
            case points.lobby: MoveTo(Lobby); break;
            case points.search: MoveTo(Search); break;
            default: break;
        }
    }

    void MoveTo(pointClass _point)
    {
        if (C_move != null) StopCoroutine(C_move);
        C_move = StartCoroutine(MoveTo_Co(_point));
    }

    IEnumerator MoveTo_Co(pointClass _point)
    {
        float _timer = 0;
        T_camHolder.parent = _point.T_point;
        Vector3 _old = T_camHolder.localPosition;
        Quaternion _oldR = T_camHolder.localRotation;
        while (_timer < 1)
        {
            T_camHolder.localPosition = Vector3.Lerp(_old, Vector3.zero, AC_moveCurve.Evaluate(_timer));
            T_camHolder.localRotation = Quaternion.Slerp(_oldR, Quaternion.identity, AC_moveCurve.Evaluate(_timer));
            yield return new WaitForEndOfFrame();
            _timer += Time.deltaTime / _point.F_moveDur;
        }
        T_camHolder.localPosition = Vector3.zero;
    }
}
