using UnityEngine;

public class FlightMeshAgent : MonoBehaviour
{
    public float F_maxSpeed = 5;
    public float F_acceleration = 5;
    public float F_brakeSpeed = 5;
    public float F_turnSpeed = 90;

    public float F_baseStoppingDistance = 0.5f;

    Vector3 v3_speed;
    Vector3 v3_destination;
    Transform t_destination;
    float f_stoppingDistance = 0;
    [HideInInspector] public bool isStopped = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isStopped)
            return;
        Movement();
    }

    void Movement()
    {
        if (t_destination != null)
            v3_destination = t_destination.position;
        Vector3 _tarDir = v3_destination - transform.position;

        Turn(_tarDir);

        if (BrakeCheck(_tarDir))
            Braking();
        else
            Accelerate(_tarDir);

        transform.position += v3_speed * Time.deltaTime;
    }
    void Turn(Vector3 _tarDir)
    {
        Quaternion tarRot = Quaternion.LookRotation(_tarDir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, tarRot, F_turnSpeed * Time.deltaTime);
    }
    bool BrakeCheck(Vector3 _tarDir)
    {
        if (Vector3.Dot(_tarDir, v3_speed) < 0)
            return true;
        if (Vector3.Distance(transform.position, v3_destination) < f_stoppingDistance)
            return true;
        return false;
    }
    void Braking()
    {
        v3_speed = Vector3.MoveTowards(v3_speed, Vector3.zero, F_brakeSpeed * Time.deltaTime);
    }
    void Accelerate(Vector3 _tarDir)
    {
        v3_speed += _tarDir * F_acceleration * Time.deltaTime;
        v3_speed = Vector3.ClampMagnitude(v3_speed, F_maxSpeed);
    }
    public void SetDestination(Transform _dest, float? _stoppingDistance = null)
    {
        t_destination = _dest;
        if (_stoppingDistance != null)
            f_stoppingDistance = _stoppingDistance.Value;
        else
            f_stoppingDistance = F_baseStoppingDistance;
    }
    public void SetDestination(Vector3 _dest, float? _stoppingDistance = null)
    {
        t_destination = null;
        v3_destination = _dest;
        if (_stoppingDistance != null)
            f_stoppingDistance = _stoppingDistance.Value;
        else
            f_stoppingDistance = F_baseStoppingDistance;
    }
    public bool AtDestination()
    {
        return Vector3.Distance(transform.position, v3_destination) < f_stoppingDistance;
    }
    private void OnEnable()
    {
        v3_speed = Vector3.zero;
    }
}
