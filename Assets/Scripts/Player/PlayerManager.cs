using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using UnityEngine.Events;

public class PlayerManager : MonoBehaviour
{
    public static PlayerController main;
    public static Conversation conversation;
    public static PlayerManager Instance;
    public List<BaseInfo> Players = new List<BaseInfo>();
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

    public void AddPlayer(BaseInfo _player)
    {
        if (!Players.Contains(_player))
        {
            Players.Add(_player);
            if (_player.isOwner)
            {
                main = _player.GetController();
                conversation = main.conversation;
                OnMainSet?.Invoke();
            }
            CheckMain(UpdatePlayerFollowers);
        }
    }
    public void RemovePlayer(BaseInfo _player)
    {
        if (Players.Contains(_player))
        {
            Players.Remove(_player);
            if (main == _player.GetController())
            {
                main = null;
            }
            else
                main.RemoveFollower(_player.GetController());
        }
    }
    void UpdatePlayerFollowers()
    {
        foreach (BaseInfo _player in Players)
        {
            PlayerController PC = _player.GetController();
            if (PC != null && PC != main)
                if (!main.CheckFollower(PC))
                    main.AddFollower(PC);
        }
    }

    public delegate void OnMainEvent();
    public event OnMainEvent OnMainSet;
    public void CheckMain(OnMainEvent _event)
    {
        if (main == null)
            OnMainSet += _event;
        else
            _event.Invoke();
    }

    public void FollowPlayer(AgentController _agent, bool _friendly = false)
    {
        StartCoroutine(FollowPlayer_Co(_agent, _friendly));
    }
    public IEnumerator FollowPlayer_Co(AgentController _agent, bool _friendly)
    {
        while (main == null)
            yield return new WaitForEndOfFrame();
        if (!_friendly)
        {
            _agent.attackTarget.FollowPlayer(main);
        }
        else
        {
            _agent.followTarget.FollowPlayer(main);
            main.AddFollower(_agent);
        }
    }
}
