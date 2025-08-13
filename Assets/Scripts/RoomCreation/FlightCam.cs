using UnityEngine;

public class FlightCam : MonoBehaviour
{
    public float latSpeed = 5f;
    public float rotSpeed = 90f;

    [HideInInspector] public Vector3 v3_camDir;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        v3_camDir = transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            CamMove();
            CamRotate();
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
            Cursor.lockState = CursorLockMode.None;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(v3_camDir), Time.deltaTime * 10);
    }

    void CamMove()
    {
        Vector3 dir = new Vector3(
            (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
            (Input.GetKey(KeyCode.E) ? 1 : 0) - (Input.GetKey(KeyCode.Q) ? 1 : 0),
            (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)
            );
        dir *= latSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            dir *= 2;
        dir *= Time.deltaTime;
        dir = transform.rotation * dir;
        transform.position += dir;
    }
    void CamRotate()
    {
        float _mult = Time.deltaTime * rotSpeed;
        Vector2 v2_camInputDir = Input.mousePositionDelta;
        v3_camDir += new Vector3(-v2_camInputDir.y, v2_camInputDir.x, 0) * _mult;
        if (v3_camDir.y > 180)
        {
            v3_camDir.y -= 360;
            transform.eulerAngles += new Vector3(0, -360, 0);
        }
        if (v3_camDir.y < -180)
        {
            v3_camDir.y += 360;
            transform.eulerAngles += new Vector3(0, 360, 0);
        }
        v3_camDir.x = Mathf.Clamp(v3_camDir.x, -80, 80);
    }
}
