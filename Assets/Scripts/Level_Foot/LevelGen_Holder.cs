using PurrNet;
using PurrNet.Packing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Mission;
using static Themes;

public class LevelGen_Holder : NetworkBehaviour
{
    public static LevelGen_Holder Instance;
    public LevelGen PF_levelGen;
    public SpaceGenerator spaceGen;
    public PlayerSpawner playerSpawner;
    public List<LevelGen> List = new List<LevelGen>();
    public SyncVar<List<_networkTransform>> Transforms = new SyncVar<List<_networkTransform>>();
    public SyncVar<string> lastLandingSpot = new SyncVar<string>();
    public SyncVar<Themes.themeEnum> theme = new SyncVar<Themes.themeEnum>(Themes.themeEnum.none);

    public List<Treasure_Point> TP_loosePoints = new List<Treasure_Point>();

    public SyncVar<int> I_value = new(0);

    public CanvasGroup CG_loadingScreen;

    bool _setup = false;
    [System.Serializable]
    public class _networkList
    {
        public List<NetworkTransform> list = new List<NetworkTransform>();
    }
    [System.Serializable]
    public class _networkTransform
    {
        public int lgID = -1;
        [Space(10)]
        public uint seed = 0;
        [Space(10)]
        public List<Vector3> position = new List<Vector3>();
        public List<Quaternion> rotation = new List<Quaternion>();
        public float _timer = 0;
    }

    public int I_maxDecals = 100;
    private List<Decal_Handler> d_Handlers = new List<Decal_Handler>();
    [HideInInspector] public LevelGen_Block shipBlock = null;

    [HideInInspector] public bool isReady = false;

    void Awake()
    {
        Instance = this;
        Transforms.onChanged += UpdateNetworkTransforms;
        theme.onChanged += Setup;
        lastLandingSpot.onChanged += Setup;
        I_value.onChanged += I_valueChanged;
    }

    private void Start()
    {
        ShowLoadingScreen_MissionLess();
    }

    void ShowLoadingScreen_MissionLess()
    {
        if (SaveData.missionCurrent == null)
        {
            CG_loadingScreen.gameObject.SetActive(true);
            CG_loadingScreen.alpha = 1;
        }
    }
    void ShowLoadingScreen_Mission()
    {
        if (SaveData.missionCurrent != null)
        {
            MainMenu.Instance.Open(MainMenu.panelEnum.loadLevel);
        }
    }

    private void FixedUpdate()
    {
        if (!_setup) return;
        foreach (var t in Transforms.value)
            UpdateTransform(t);
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        if (isController)
        {
            ShowLoadingScreen_Mission();
            theme.value = SaveData.themeCurrent;
            lastLandingSpot.value = SaveData.lastLandingSpot;
            Setup();
            CreateNew();
        }
    }

    protected override void OnDestroy()
    {
        Transforms.onChanged -= UpdateNetworkTransforms;
        theme.onChanged -= Setup;
        lastLandingSpot.onChanged -= Setup;
        I_value.onChanged -= I_valueChanged;
    }

    public int GetCollectedValue()
    {
        int _value = 0;
        foreach (var LG in List)
        {
            _value += LG.GetCollectedValue();
        }
        foreach (var LG in TP_loosePoints)
        {
            _value += LG.I_value;
        }
        return _value;
    }

    public void I_valueChanged(int _amt) { CheckReady(UpdateCollectedValueDisplay); }
    public void UpdateCollectedValueDisplay()
    {
        foreach (var LG in List)
        {
            foreach (var block in LG.LG_Blocks)
            {
                foreach (var point in block.TP_TreasurePoints)
                {
                    point.DisplayPoints(I_value.value);
                }
            }
        }
        foreach (var TP in TP_loosePoints)
        {
            TP.DisplayPoints(I_value.value);
        }
    }

    [ObserversRpc]
    public static void LoadTheme(Themes.themeEnum _theme, int _mission = -1)
    {
        SaveData.themeCurrent = _theme;
        if (_mission == -1)
            SaveData.missionCurrent = null;
        else
            SaveData.missionCurrent = Objective.Instance.GetMission(_mission).Clone();
        SaveData.i_currency += Instance.GetCollectedValue();
        SceneManager.LoadScene(1);
    }

    [ObserversRpc]
    public void EndLevel(Themes.themeEnum _tarTheme)
    {
        ShowEndScreen(_tarTheme);
    }
    void ShowEndScreen(Themes.themeEnum _tarTheme)
    {
        if (SaveData.missionCurrent == null)
        {
            LoadTheme(_tarTheme);
        }
        else
        {
            MainMenu.Instance.endLevel.stats = PlayerManager.main.Stats;
            foreach (var C in PlayerManager.Instance.Controllers)
                if (C != null)
                    C.gameObject.SetActive(false);
            foreach (var LG in List)
                LG.gameObject.SetActive(false);

            MainMenu.Instance.endLevel.themeEnum = _tarTheme;
            MainMenu.Instance.Open(MainMenu.panelEnum.endLevel);
            
        }
    }
    void Setup<T>(T _seed)
    {
        if (theme != Themes.themeEnum.none &&
            lastLandingSpot != "")
            Setup();
    }
    void Setup()
    {
        if (_setup)
            return;
        _setup = true;
        SaveData.themeCurrent = theme;
        SaveData.lastLandingSpot = lastLandingSpot;
        if (theme == Themes.themeEnum.ship)
            spaceGen.Generate();
        UpdateNetworkTransforms(Transforms);
    }

    public void UpdateNetworkTransforms(List<_networkTransform> _transforms)
    {
        if (!_setup)
            return;
        for (int i = 0; i < _transforms.Count; i++)
        {
            if (i >= List.Count)
            {
                CreateNew(_transforms[i]);
                continue;
            }
        }
    }

    public void CreateNew(_networkTransform _transform)
    {
        LevelGen LG = Instantiate(PF_levelGen, transform);

        Vector3 _pos = Vector3.zero;
        if (_transform.position.Count > 0)
            _pos = _transform.position[0];

        LG.Setup(_transform.seed, List.Count, _pos);
        List.Add(LG);
        RequestTransform(List.Count - 1, NetworkManager.main.localPlayer);
    }
    public void CreateNew(bool _player = true)
    {
        Vector3 _pos = Vector3.zero;
        if (_player)
        {
            if (SaveData.themeCurrent == themeEnum.ship)
            {
                Vector3 pos;
                if (spaceGen.GetExitPos(out pos, SaveData.lastLandingSpot))
                    _pos = pos;
            }
        }
        CreateNew(_pos, null, _player);
    }
    public void CreateNew(Vector3 _pos, Layout_Defined _layout = null, bool _player = true, BoxCollider _BC = null)
    {
        _networkTransform _transform = new _networkTransform();
        _transform.lgID = Transforms.value.Count;
        //_transform.UpdateTransform(Vector3.zero, Quaternion.identity);
        _transform.seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
        Transforms.value.Add(_transform);

        LevelGen LG = Instantiate(PF_levelGen, transform);
        LG.isHost = true;
        if (_layout == null)
            LG.Setup(_transform.seed, List.Count, _pos, _player, _BC);
        else
            LG.Setup(_transform.seed, List.Count, _pos, _layout, _player, _BC);
        List.Add(LG);
    }

    [ObserversRpc]
    public void UpdateTransform(int _lgID, Vector3 pos, Quaternion rot)
    {
        if (_lgID < 0 || _lgID >= Transforms.value.Count)
            return;
        _networkTransform nt = Transforms.value[_lgID];
        nt.position.Add(pos);
        nt.rotation.Add(rot);
    }

    public void UpdateTransform(_networkTransform _transform)
    {
        Transform T = List[_transform.lgID].T_Holder;
        if (!T)
            return;
        if (!List[_transform.lgID].ship)
            return;
        if (List[_transform.lgID].ship.DriverIsMain())
            return;
        if (List[_transform.lgID].ship.DriverIsAgent() && isHost)
            return;
        if (_transform.position.Count == 0)
        {
            _transform._timer = 0;
            return;
        }
        Vector3 lastPos = _transform.position[0];
        Quaternion lastRot = _transform.rotation[0];
        if (_transform.position.Count == 1)
        {
            T.position = lastPos;
            T.rotation = lastRot;
            _transform._timer = 0;
            return;
        }
        float mult = 0.6f + (0.2f * _transform.position.Count);
        _transform._timer += NetworkManager.main.tickModule.tickDelta * (float)NetworkManager.main.tickModule.tickRate * mult;
        Vector3 tarPos = _transform.position[1];
        Quaternion tarRot = _transform.rotation[1];
        T.position = Vector3.Lerp(lastPos, tarPos, _transform._timer);
        T.rotation = Quaternion.Lerp(lastRot, tarRot, _transform._timer);
        if (_transform._timer >= 1)
        {
            int amt = Mathf.FloorToInt(_transform._timer);
            amt = Mathf.Min(amt, _transform.position.Count - 1);
            _transform._timer = _transform._timer % 1;
            _transform.position.RemoveRange(0, amt);
            _transform.rotation.RemoveRange(0, amt);
        }
    }
    [ServerRpc]
    public void RequestTransform(int _lgID, PlayerID _player)
    {
        if (_lgID < 0 || _lgID >= Transforms.value.Count)
            return;

        Transform T = List[_lgID].T_Holder;
        if (!T)
            return;
        SendTransform(_player, _lgID, T.position, T.rotation);
    }
    [TargetRpc]
    public void SendTransform(PlayerID _player, int _lgID, Vector3 pos, Quaternion rot)
    {
        if (_lgID < 0 || _lgID >= Transforms.value.Count)
            return;
        Transforms.value[_lgID].position.Add(pos);
        Transforms.value[_lgID].rotation.Add(rot);
    }
    public void UpdateTransform(LevelGen LG)
    {
        if (LG == null) return;
        UpdateTransform(
            LG.id,
            LG.T_Holder.position,
            LG.T_Holder.rotation
            );
    }

    public void AddDecal(Decal_Handler handler)
    {
        d_Handlers.Add(handler);
        if (d_Handlers.Count > I_maxDecals)
        {
            d_Handlers[0].Remove();
            d_Handlers.RemoveAt(0);
        }
    }

    public void RemoveDecal(Decal_Handler handler)
    {
        if (d_Handlers.Contains(handler))
            d_Handlers.Remove(handler);
    }

    public void AgentDeath(AgentController _AC)
    {
        int _amtRemaining = 0;
        foreach (var LG in List)
        {
            LG.AgentDeath(_AC);
            _amtRemaining += LG.GetEnemyAmount();
        }
        if (SaveData.missionCurrent)
            SaveData.missionCurrent.SpawnEnemies(
                Mission.eventEnum.enemyRemaining,
                null,
               _amtRemaining);
    }
    public void IsReady()
    {
        if (isReady)
            return;
        isReady = true;

        playerSpawner.canSpawn = true;
        onReadyEvent?.Invoke();
        if (SaveData.missionCurrent)
            SaveData.missionCurrent.OnLevelLoaded();
        else
            OnLevelLoaded_NoMission();
        StartCoroutine(FadeLoadingScreen());
    }

    void OnLevelLoaded_NoMission()
    {
        List<LevelGen_Spawn> spawn = new List<LevelGen_Spawn>();
            spawn.AddRange(LevelGen_Holder.Instance.GetSpawns(
                eventEnum.levelLoaded,
                LevelGen_Spawn.spawnTypeEnum.companion));

        foreach (var item in spawn)
            item.Spawn();
    }

    public List<LevelGen_Spawn> GetSpawns(Mission.eventEnum _event, LevelGen_Spawn.spawnTypeEnum _spawn)
    {
        List<LevelGen_Spawn> _list = new List<LevelGen_Spawn>();
        foreach(var LG in List)
            _list.AddRange(LG.GetSpawns(_event, _spawn));
        return _list;
    }


    public delegate void OnReadyEvent();
    public event OnReadyEvent onReadyEvent;
    public void CheckReady(OnReadyEvent _event)
    {
        if (!isReady)
            onReadyEvent += _event;
        else
            _event.Invoke();
    }

    IEnumerator FadeLoadingScreen(float _dur = 1f)
    {
        MainMenu.Instance.loadingLevel.Close();
        float _timer = 0f;
        while (_timer < 1f)
        {
            CG_loadingScreen.alpha = Mathf.Lerp(1f, 0f, _timer);
            yield return new WaitForEndOfFrame();
            _timer += Time.deltaTime / _dur;
        }
        CG_loadingScreen.alpha = 0f;
        CG_loadingScreen.gameObject.SetActive(false);
    }

    public bool GetRoom(out LevelGen_Block _LGB, Vector3Int _V3ID)
    {
        _LGB = null;
        if (_V3ID.x < 0) return false;
        if (_V3ID.y < 0) return false;
        if (_V3ID.x >= LevelGen_Holder.Instance.List.Count) return false;
        if (_V3ID.y >= LevelGen_Holder.Instance.List[_V3ID.x].LG_Blocks.Count) return false;
        _LGB = LevelGen_Holder.Instance.List[_V3ID.x].LG_Blocks[_V3ID.y];
        return true;
    }
}
