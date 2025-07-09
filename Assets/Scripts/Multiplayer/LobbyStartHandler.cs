using PurrLobby;
using UnityEngine;

public class LobbyStartHandler : MonoBehaviour
{
    [SerializeField] private LobbyManager LM;
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
}
