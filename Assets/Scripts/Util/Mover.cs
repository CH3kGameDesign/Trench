using UnityEngine;

public class Mover : MonoBehaviour
{
    public Vector3 V3_rotPerSecond = Vector3.zero;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles += V3_rotPerSecond * Time.deltaTime;
    }
}
