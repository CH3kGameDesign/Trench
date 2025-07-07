using PurrNet;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LevelGen))]
public class LevelGen_Seed : NetworkBehaviour
{
    public SyncVar<uint> seed = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitForHost());
        seed.onChanged += GetComponent<LevelGen>().Setup;
    }

    protected override void OnDestroy()
    {
        seed.onChanged -= GetComponent<LevelGen>().Setup;
        base.OnDestroy();
    }

    IEnumerator WaitForHost()
    {
        while (seed == uint.MinValue)
        {
            if (isHost)
                seed.value = (uint)UnityEngine.Random.Range(1, int.MaxValue);
            yield return new WaitForEndOfFrame();
        }
    }
}
