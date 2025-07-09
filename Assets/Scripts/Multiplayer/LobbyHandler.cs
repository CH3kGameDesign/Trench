using PurrLobby;
using PurrLobby.Providers;
using PurrNet;
using PurrNet.Logging;
using PurrNet.Steam;
using PurrNet.Transports;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LobbyHandler : MonoBehaviour
{
    [SerializeField] private LobbyManager LM;
    [SerializeField] private SteamLobbyProvider SLP;
    private LobbyDataHolder _lobbyDataHolder;
    [Space(10)]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private UDPTransport udpTransport;
    [SerializeField] private SteamTransport steamTransport;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();
        if (!isServer())
        {
            if (SLP.IsSteamClientAvailable) SetupSteamServer();
            else SetupUDPServer();
            networkManager.StartServer();
        }
        StartCoroutine(StartClient());
    }
    void SetupSteamServer()
    {
        //networkManager.transport = steamTransport;
        LM.CreateRoom();
    }

    void SetupUDPServer()
    {
        networkManager.transport = udpTransport;
    }

    IEnumerator StartClient()
    {
        yield return new WaitForSecondsRealtime(1);
        if (_lobbyDataHolder)
        {
            steamTransport.address = _lobbyDataHolder.CurrentLobby.Members[0].Id;
        }
        networkManager.StartClient();
    }

    bool isServer()
    {
        bool isServer = !_lobbyDataHolder;

#if UNITY_EDITOR
        isServer = isServer && !ParrelSync.ClonesManager.IsClone();
#endif
        return isServer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region Public Methods
    public void OnRoomJoined(Lobby _lobby)
    {
        if (steamTransport.address != _lobbyDataHolder.CurrentLobby.Members[0].Id)
           SceneManager.LoadScene(0);
    }
    public void OnRoomJoinFailed(string _string)
    {

    }
    public void OnRoomLeft()
    {

    }
    public void OnRoomUpdated(Lobby _lobby)
    {
        steamTransport.address = _lobbyDataHolder.CurrentLobby.Members[0].Id;
    }
    public void OnPlayerListUpdated(List<LobbyUser> _list)
    {
        Debug.Log(_list.Count);
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
