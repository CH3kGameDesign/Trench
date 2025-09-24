using System.Collections.Generic;
using PurrLobby;
using PurrNet;
using Unity.VisualScripting;

public class LoadingArea : NetworkBehaviour
{
    public List<RagdollManager> RM_players = new List<RagdollManager>();

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
        LobbyDataHolder _lobby = FindFirstObjectByType<LobbyDataHolder>();
        bool _fakeLobby = true;
        if (_lobby)
        {
            List<LobbyUser> _users = _lobby.CurrentLobby.Members;
            if (_users != null)
            {
                _fakeLobby = false;
                string[] _names = new string[_users.Count];
                for (int i = 0; i < _names.Length; i++)
                    _names[i] = _users[i].DisplayName;

                MainMenu.Instance.loadingLevel.SetNames(_names);
            }
        }
        if (_fakeLobby)
        {
            string[] _names = new string[networkManager.playerCount];
            for (int i = 0; i < _names.Length; i++)
                _names[i] = "Player";
            MainMenu.Instance.loadingLevel.SetNames(_names);
        }
        if (isHost || isServer)
        {
            if (networkManager.playerCount > 0)
            {
                ShowPlayers(networkManager.playerCount);
                for (int i = 0; i < networkManager.playerCount; i++)
                    LoadArmor(networkManager.players[i], i);
            }
            else
            {
                LoadArmor(0);
            }
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
            RM_players[i].gameObject.SetActive(true);
        }
    }
}
