using System;
using System.Collections.Generic;
using PurrLobby;
using PurrNet;

public class LoadingArea : NetworkBehaviour
{
    public List<RagdollManager> RM_players = new List<RagdollManager>();

    public stateEnum _state = stateEnum.loadLevel;
    bool _loaded = false;
    public enum stateEnum { loadLevel, endLevel};
    private void Awake()
    {
        foreach (var item in RM_players)
        {
            item.gameObject.SetActive(false);
        }
    }
    protected override void OnSpawned()
    {
        base.OnSpawned();
        Setup();
    }

    public void Setup()
    {
        if (_loaded) return;
        LobbyDataHolder _lobby = FindFirstObjectByType<LobbyDataHolder>();

        switch (_state)
        {
            case stateEnum.loadLevel:
                LoadingLevel(_lobby);
                break;
            case stateEnum.endLevel:
                EndLevel(_lobby);
                break;
            default:
                break;
        }
        _loaded = true;
    }

    void LoadingLevel(LobbyDataHolder _lobby)
    {
        bool _fakeLobby = true;
        if (_lobby)
        {
            List<LobbyUser> _users = _lobby.CurrentLobby.Members;
            if (_users != null)
            {
                if (_users.Count > 0)
                {
                    _fakeLobby = false;
                    string[] _names = new string[_users.Count];
                    for (int i = 0; i < _names.Length; i++)
                        _names[i] = _users[i].DisplayName;

                    MainMenu.Instance.loadingLevel.SetNames(_names);
                }
            }
        }
        if (_fakeLobby)
        {
            string[] _names = new string[Math.Max(NetworkManager.main.playerCount, 1)];
            for (int i = 0; i < _names.Length; i++)
                _names[i] = "Player";
            MainMenu.Instance.loadingLevel.SetNames(_names);
        }
        if (NetworkManager.main.isServer || NetworkManager.main.isHost)
        {
            if (NetworkManager.main.playerCount > 0)
            {
                ShowPlayers(NetworkManager.main.playerCount);
                for (int i = 0; i < NetworkManager.main.playerCount; i++)
                    LoadArmor(NetworkManager.main.players[i], i);
            }
            else
            {
                ShowPlayers(1);
                LoadArmor(0);
            }
        }
    }
    void EndLevel(LobbyDataHolder _lobby)
    {
        bool _fakeLobby = true;
        if (_lobby)
        {
            List<LobbyUser> _users = _lobby.CurrentLobby.Members;
            if (_users != null)
            {
                if (_users.Count > 0)
                {
                    _fakeLobby = false;
                    string[] _names = new string[_users.Count];
                    for (int i = 0; i < _names.Length; i++)
                        _names[i] = _users[i].DisplayName;

                    MainMenu.Instance.endLevel.SetNames(_names);
                }
            }
        }
        if (_fakeLobby)
        {
            string[] _names = new string[Math.Max(NetworkManager.main.playerCount, 1)];
            for (int i = 0; i < _names.Length; i++)
                _names[i] = "Player";
            MainMenu.Instance.endLevel.SetNames(_names);
        }
        if (isHost || isServer)
        {
            //Get Dead Players
            List<BaseInfo> _players = PlayerManager.Instance.Players;
            List<int> _i = new List<int>();
            for (int i = 0; i < _players.Count; i++)
                if (!_players[i].b_alive)
                    _i.Add(i);
            //Show Dead Players
            ShowPlayers(_i.Count);
            for (int i = 0; i < _i.Count; i++)
                LoadArmor(NetworkManager.main.players[_i[i]], i);
        }
    }

    [TargetRpc]
    public void LoadArmor(PlayerID _id, int _i)
    {
        LoadArmor(_i);
    }
    public void LoadArmor(int _i)
    {
        if (_i >= RM_players.Count)
            return;

        RM_players[_i].gameObject.SetActive(true);
        ArmorManager.Instance.EquipArmor(RM_players[_i], SaveData.equippedArmor);
    }
    [ObserversRpc]
    public void ShowPlayers(int _amt)
    {
        for (int i = 0; i < RM_players.Count; i++)
        {
            RM_players[i].gameObject.SetActive(i < _amt);
        }
    }
    [ObserversRpc]
    public void ShowDeadPlayers(int _amt)
    {
        for (int i = 0; i < RM_players.Count; i++)
        {
            RM_players[i].gameObject.SetActive(i < _amt);
        }
    }
}
