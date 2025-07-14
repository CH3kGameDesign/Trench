using PurrNet;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LevelGen))]
public class LevelGen_Seed : NetworkBehaviour
{
    public SyncVar<uint> seed = new();
    public SyncVar<Themes.themeEnum> themeCurrent = new SyncVar<Themes.themeEnum>(Themes.themeEnum.none);
    public SyncVar<string> lastLandingSpot = new SyncVar<string>("");
    public LevelGen levelGen;
    public SpaceGenerator spaceGen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        seed.onChanged += Setup;
        themeCurrent.onChanged += Setup;
        lastLandingSpot.onChanged += Setup;
    }
    protected override void OnSpawned()
    {
        if (isController)
        {
            levelGen.isHost = true;
            seed.value = (uint)UnityEngine.Random.Range(1, int.MaxValue);
            themeCurrent.value = SaveData.themeCurrent;
            lastLandingSpot.value = SaveData.lastLandingSpot;
        }
        base.OnSpawned();
    }

    protected override void OnDestroy()
    {
        seed.onChanged -= Setup;
        themeCurrent.onChanged -= Setup;
        lastLandingSpot.onChanged -= Setup;
        base.OnDestroy();
    }

    void Setup<T>(T _seed) {
        if (themeCurrent != Themes.themeEnum.none &&
            seed != 0 &&
            lastLandingSpot != "")
            Setup();
    }
    void Setup()
    {
        SaveData.themeCurrent = themeCurrent.value;
        SaveData.lastLandingSpot = lastLandingSpot.value;
        if (SaveData.themeCurrent == Themes.themeEnum.ship)
            spaceGen.Generate();
        levelGen.Setup(seed, 0);
    }
}
