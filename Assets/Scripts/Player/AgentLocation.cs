using UnityEngine;
using System.Collections.Generic;

public class AgentLocation : MonoBehaviour
{
    public RagdollManager RM;
    public List<LevelGen_Bounds> LGBS = new List<LevelGen_Bounds>();
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
        {
            if (!LGBS.Contains(LGB))
                LGBS.Add(LGB);
            UpdateRoom();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        LevelGen_Bounds LGB;
        if (other.TryGetComponent<LevelGen_Bounds>(out LGB))
        {
            if (LGBS.Contains(LGB))
                LGBS.Remove(LGB);
            UpdateRoom();
        }
    }

    public void ClearRooms()
    {
        LGBS.Clear();
        UpdateRoom();
    }

    void UpdateRoom()
    {
        for (int i = LGBS.Count - 1; i >= 0; i--)
        {
            RM.controller.CheckReady(delegate { RM.controller.UpdateRoom(LGBS[i]); });
            return;
        }
        RM.controller.CheckReady(delegate { RM.controller.UpdateRoom(null); });
    }
}
