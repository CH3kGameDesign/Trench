using UnityEngine;
using UnityEngine.SceneManagement;

public class Ship : Vehicle
{
    public override void OnInteract(BaseController _player)
    {
        SceneManager.LoadScene(0);
    }
}
