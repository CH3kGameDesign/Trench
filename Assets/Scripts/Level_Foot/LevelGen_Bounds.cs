using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelGen_Bounds : MonoBehaviour
{
    public BoxCollider B_Bounds;
    [HideInInspector] public int I_roomNum = -1;
    [HideInInspector] public Vector3Int V3ID = Vector3Int.left;
    public eventClass OnEnter_Event;
    [System.Serializable]
    public class eventClass
    {
        public bool _activated = false;
        public UnityEvent<LevelGen_Bounds> _event = new UnityEvent<LevelGen_Bounds>();
        public bool _singleUse = true;
        public float _delay = 0f;

        public void Activate(LevelGen_Bounds _bounds)
        {
            if (_singleUse && _activated)
                return;
            _activated = true;
            if (_delay > 0f)
                _bounds.StartCoroutine(Activate_Delay(_delay,_bounds));
            else
                _event.Invoke(_bounds);
        }

        public IEnumerator Activate_Delay(float _delay, LevelGen_Bounds _bounds)
        {
            yield return new WaitForSeconds(_delay);
            _event.Invoke(_bounds);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        OnEnter_Event._activated = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(BoxCollider BC)
    {
        B_Bounds = BC;
        BC.isTrigger = true;
        gameObject.layer = 12;
    }
    public void SetID(Vector3Int _V3ID)
    {
        I_roomNum = _V3ID.y;
        V3ID = _V3ID;
    }

    public void OnEnter(AgentLocation _agent)
    {
        if (_agent.RM.controller != PlayerManager.main)
            return;
        OnEnter_Event.Activate(this);
    }
}
