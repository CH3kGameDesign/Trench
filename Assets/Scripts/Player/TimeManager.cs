using UnityEngine;
using PurrNet;

public class TimeManager : NetworkBehaviour
{
    public static SyncVar<float> TimeScale = new (1f);

    protected void Awake()
    {
        TimeScale.onChanged += SetTimeScale;
        onObserverAdded += PlayerAdded;
    }
    protected override void OnDestroy()
    {
        TimeScale.onChanged -= SetTimeScale;
        onObserverAdded -= PlayerAdded;
    }
    [ObserversRpc]
    public void SetTimeScale(float value)
    {
        if (networkManager.playerCount > 1)
            value = 1f;

        Time.timeScale = value;
    }
    public void PlayerAdded(PlayerID _player)
    {
        TimeScale.value = 1f;
    }
}
