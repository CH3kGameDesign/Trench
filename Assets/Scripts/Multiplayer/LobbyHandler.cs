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
    [SerializeField] private bool ignoreLobby = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isClone())
            return;
        _lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();

        if (SLP.IsSteamClientAvailable) SetupSteamServer();
        else SetupUDPServer();

        if (isServer())
            networkManager.StartServer();

        StartCoroutine(StartClient());
    }
    void SetupSteamServer()
    {
        //networkManager.transport = steamTransport;
        if (!LM.CurrentLobby.IsValid)
            LM.CreateRoom();
    }

    void SetupUDPServer()
    {
        ignoreLobby = true;
        //networkManager.transport = udpTransport;
    }

    private void OnApplicationQuit()
    {
        LM.LeaveLobby();
    }

    IEnumerator StartClient()
    {
        yield return new WaitForSecondsRealtime(1);
        if (_lobbyDataHolder && !ignoreLobby)
        {
            while (_lobbyDataHolder.CurrentLobby.IsValid == false)
                yield return new WaitForEndOfFrame();
            steamTransport.address = _lobbyDataHolder.CurrentLobby.Members[0].Id;
        }
        networkManager.StartClient();
    }

    bool isServer()
    {
        bool isServer = true;
        if (_lobbyDataHolder)
            isServer = _lobbyDataHolder.CurrentLobby.IsOwner || !_lobbyDataHolder.CurrentLobby.IsValid;

        return isServer;
    }

    bool isClone()
    {
        bool isClone = false;

#if UNITY_EDITOR
        isClone = ParrelSync.ClonesManager.IsClone();
#endif
        return isClone;
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
