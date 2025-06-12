using UnityEngine;

public class AgentLocation : MonoBehaviour
{
    public BaseController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        LevelGen_Bounds LGB;
        if (other.TryGetComponent<LevelGen_Bounds>(out LGB))
            controller.UpdateRoom(LGB.I_roomNum);
    }
}
