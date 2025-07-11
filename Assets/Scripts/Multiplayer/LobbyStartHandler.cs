using PurrLobby;
using PurrLobby.Providers;
using PurrNet;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class LobbyStartHandler : MonoBehaviour
{
    [SerializeField] private LobbyManager LM;
    [SerializeField] private SteamLobbyProvider SLP;
    [SerializeField] private ViewManager VW;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        LM.LeaveLobby();
    }

    // Update is called once per frame
    void Update()
    {
        
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
            LM.CreateRoom();
            VW.OnRoomCreateClicked();
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
