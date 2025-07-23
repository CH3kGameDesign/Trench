using PurrLobby;
using PurrLobby.Providers;
using PurrNet;
using PurrNet.Transports;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyStartHandler : MonoBehaviour
{
    [SerializeField] private LobbyManager LM;
    [SerializeField] private SteamLobbyProvider SLP;
    [SerializeField] private ViewManager VW;

    public RagdollManager[] playerBodies = new RagdollManager[0];

    public Button[] B_multiplayerButtons;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateSteamButtons();
        UpdateArmors();

        LM.LeaveLobby();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateSteamButtons()
    {
        bool _active = SLP.IsSteamClientAvailable;
        foreach (var button in B_multiplayerButtons)
        {
            button.interactable = _active;  
        }
    }

    private void OnApplicationQuit()
    {
        LM.LeaveLobby();
    }

    public void StartLocal()
    {
        SceneManager.LoadScene(1);
    }
    public void StartLobby()
    {
        if (SLP.IsSteamClientAvailable)
        {
            Camera_MainMenu.Instance.MoveTo(Camera_MainMenu.points.lobby);
            LM.CreateRoom();
            VW.OnRoomCreateClicked();
            UpdatePlayers();
        }
    }

    public void JoinRoom()
    {
        Camera_MainMenu.Instance.MoveTo(Camera_MainMenu.points.lobby);
    }
    public void Browse()
    {
        VW.OnBrowseClicked();
        Camera_MainMenu.Instance.MoveTo(Camera_MainMenu.points.search);
    }
    public void Leave()
    {
        Camera_MainMenu.Instance.MoveTo(Camera_MainMenu.points.main);
        HidePlayers();
    }
    public void Settings()
    {
        VW.OnSettingsClicked();
        Camera_MainMenu.Instance.MoveTo(Camera_MainMenu.points.settings);
    }
    public void Back()
    {
        VW.OnRoomLeft();
        Camera_MainMenu.Instance.MoveTo(Camera_MainMenu.points.main);
    }
    public void LobbyUpdate(Lobby _lobby)
    {
        UpdatePlayers(_lobby.Members);
    }

    public void UpdateArmors()
    {
        for (int i = 0; i < playerBodies.Length; i++)
        {
            UpdateArmor(playerBodies[i]);
        }
    }
    public void UpdateArmor(RagdollManager _player)
    {
        ArmorManager.Instance.EquipArmor(_player, SaveData.equippedArmor);
    }

    public void UpdatePlayers()
    {
        for (int i = 0; i < playerBodies.Length; i++)
        {
            if (i > 0)
            {
                playerBodies[i].gameObject.SetActive(false);
                continue;
            }
            playerBodies[i].gameObject.SetActive(true);
        }
    }
    public void UpdatePlayers(List<LobbyUser> _players)
    {
        for (int i = 0; i < playerBodies.Length; i++)
        {
            if (i >= _players.Count)
            {
                playerBodies[i].gameObject.SetActive(false);
                continue;
            }
            playerBodies[i].gameObject.SetActive(true);
        }
    }
    public void HidePlayers()
    {
        for (int i = 0; i < playerBodies.Length; i++)
        {
            playerBodies[i].gameObject.SetActive(false);
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
