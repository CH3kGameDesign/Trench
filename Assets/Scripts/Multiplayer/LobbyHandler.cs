using PurrLobby;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LobbyHandler : MonoBehaviour
{
    [SerializeField] private LobbyManager LM;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LM.CreateRoom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region Public Methods
    public void OnRoomJoined(Lobby _lobby)
    {

    }
    public void OnRoomJoinFailed(string _string)
    {

    }
    public void OnRoomLeft()
    {

    }
    public void OnRoomUpdated(Lobby _lobby)
    {

    }
    public void OnPlayerListUpdated(List<LobbyUser> _list)
    {

    }
    public void OnRoomSearchResults(List<Lobby> _list)
    {

    }
    public void OnFriendListPulled(List<FriendUser> _list)
    {

    }
    public void OnAllReady()
    {

    }
    public void OnError(string _string)
    {

    }
    public void onInitialized()
    {

    }
    public void onShutdown()
    {

    }
    #endregion
}
