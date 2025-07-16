using PurrLobby;
using PurrLobby.Providers;
using PurrNet;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyStartHandler : MonoBehaviour
{
    [SerializeField] private LobbyManager LM;
    [SerializeField] private SteamLobbyProvider SLP;
    [SerializeField] private ViewManager VW;

    public Button[] B_multiplayerButtons;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateSteamButtons();

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
