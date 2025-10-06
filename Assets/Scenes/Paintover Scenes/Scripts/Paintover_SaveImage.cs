using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI.Table;

public class Paintover_SaveImage : MonoBehaviour
{
    public Camera C_Camera;
    // Update is called once per frame
    public void SaveImage()
    {
        string targetPath = "Assets/Scenes/Paintover Scenes/Images/" + "temp.png";
        C_Camera.Render();
#if UNITY_EDITOR
        C_Camera.activeTexture.SaveToFile(targetPath);
#endif
    }
}
