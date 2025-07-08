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
            if (_player.GetComponent<NetworkOwnershipToggle>().isOwner)
            {
                conversation = _player.conversation;
                main = _player;
                OnMainSet?.Invoke();
            }
        }
    }
    public void RemovePlayer(PlayerController _player)
    {
        if (Players.Contains(_player))
        {
            Players.Remove(_player);
            if (main == _player)
            {
                main = null;
            }
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
