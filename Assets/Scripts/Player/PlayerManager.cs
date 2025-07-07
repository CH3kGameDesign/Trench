using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerController Main;
    public static PlayerManager Instance;
    public List<PlayerController> Players = new List<PlayerController>();
    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPlayer(PlayerController _player)
    {
        if (!Players.Contains(_player))
        {
            Players.Add(_player);
            if (Main == null)
                Main = _player;
        }
    }

    public void FollowPlayer(AgentController _agent, bool _friendly = false)
    {
        StartCoroutine(FollowPlayer_Co(_agent, _friendly));
    }
    public IEnumerator FollowPlayer_Co(AgentController _agent, bool _friendly)
    {
        while (Main == null)
            yield return new WaitForEndOfFrame();
        if (!_friendly)
        {
            _agent.attackTarget.FollowPlayer(Main);
        }
        else
        {
            _agent.followTarget.FollowPlayer(Main);
            Main.AddFollower(_agent);
        }
    }
}
