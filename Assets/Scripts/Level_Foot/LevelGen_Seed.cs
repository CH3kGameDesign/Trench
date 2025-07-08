using PurrNet;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LevelGen))]
public class LevelGen_Seed : NetworkBehaviour
{
    public SyncVar<uint> seed = new();
    LevelGen levelGen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelGen = GetComponent<LevelGen>();
        StartCoroutine(WaitForHost());
        seed.onChanged += levelGen.Setup;
    }

    protected override void OnDestroy()
    {
        
    }

    IEnumerator WaitForHost()
    {
        while (seed == uint.MinValue)
        {
            if (isHost)
            {
                levelGen.isHost = true;
                seed.value = (uint)UnityEngine.Random.Range(1, int.MaxValue);
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
