using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class Camera_MainMenu : MonoBehaviour
{
    public static Camera_MainMenu Instance;

    public Transform T_camHolder;
    public Transform T_camera;
    public RectTransform RT_canvasPivot;
    public AnimCurve AC_moveCurve;
    [Space(10)]
    public pointClass Main;
    public pointClass Lobby;
    public pointClass Search;
    [System.Serializable]
    public class pointClass
    {
        public Transform T_point;

        [Range(0,1.0f)]public float F_moveDur = 0.2f;
        [Header("Hover")]
        public float hoverScale = 0.2f;
        public float hoverSpeed = 0.2f;
        public float hoverRotMax = 2f;
        public float rotSpeed = 5f;
        [Header("Objects")]
        public GameObject[] activeObjects = new GameObject[0];

        public void Enable(bool _enable = true)
        {
            foreach (var item in activeObjects)
            {
                item.SetActive(_enable);
            }
        }
    }
    private pointClass activePoint = null;
    public enum points {main, lobby, search};


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
        StartCoroutine(CameraHoverPos());
        StartCoroutine(CameraRot());
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

        if (activePoint != null)
            activePoint.Enable(false);
        activePoint = _point;
        _point.Enable();
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

    Vector3 hoverPos = Vector3.zero;
    IEnumerator CameraHoverPos()
    {
        while (true)
        {
            if (activePoint != null)
            {
                if (activePoint.hoverSpeed > 0 && activePoint.hoverScale > 0)
                {
                    hoverPos.x = Mathf.PerlinNoise(Time.time * activePoint.hoverSpeed, 0);
                    hoverPos.y = Mathf.PerlinNoise(0, Time.time * activePoint.hoverSpeed);
                    hoverPos.z = Mathf.PerlinNoise(Time.time * activePoint.hoverSpeed, Time.time * activePoint.hoverSpeed);
                    hoverPos *= activePoint.hoverScale;
                }
                else
                    hoverPos = Vector3.zero;
                T_camera.localPosition = hoverPos;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    Vector3 hoverRot = Vector3.zero;
    Vector3 canvasPos = Vector3.zero;
    Vector2 rotInput = Vector2.zero;
    public float canvasMult = 10f;
    IEnumerator CameraRot()
    {
        while (true)
        {
            if (activePoint != null)
            {
                if (activePoint.rotSpeed > 0 && activePoint.hoverRotMax > 0)
                {
                    Vector3 _input = new Vector3(-rotInput.y, rotInput.x, 0);
                    hoverRot += _input * activePoint.rotSpeed * Time.deltaTime;
                    hoverRot = Vector3.Lerp(hoverRot, Vector3.zero, Time.deltaTime);
                    hoverRot = Vector2.ClampMagnitude(hoverRot, activePoint.hoverRotMax);

                    _input = new Vector3(-rotInput.x, -rotInput.y, 0);
                    canvasPos += _input * activePoint.rotSpeed * Time.deltaTime;
                    canvasPos = Vector3.Lerp(canvasPos, Vector3.zero, Time.deltaTime);
                    canvasPos = Vector2.ClampMagnitude(canvasPos, activePoint.hoverRotMax);
                }
                else
                {
                    hoverRot = Vector3.zero;
                    canvasPos = Vector3.zero;
                }
                T_camera.localEulerAngles = hoverRot;
                RT_canvasPivot.localPosition = canvasPos * canvasMult;
            }

            yield return new WaitForEndOfFrame();
        }
    }
    public void Input_CamMovement(InputAction.CallbackContext cxt)
    {
        rotInput = Input_GetVector2(cxt);
    }
    public void Input_ChangedInput(PlayerInput input)
    {

    }
    Vector2 Input_GetVector2(InputAction.CallbackContext cxt)
    {
        switch (cxt.phase)
        {
            case InputActionPhase.Performed:
                return cxt.action.ReadValue<Vector2>();
            default:
                return Vector2.zero;
        }
    }
}
