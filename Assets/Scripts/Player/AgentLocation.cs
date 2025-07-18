using UnityEngine;

public class AgentLocation : MonoBehaviour
{
    public RagdollManager RM;
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
            RM.controller.CheckReady(delegate { RM.controller.UpdateRoom(LGB); });
    }
}
