using PurrNet;
using PurrNet.Packing;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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


    [HideInInspector] public bool isReady = false;

    void Awake()
    {
        Instance = this;
        Transforms.onChanged += UpdateNetworkTransforms;
        theme.onChanged += Setup;
        lastLandingSpot.onChanged += Setup;


        CG_loadingScreen.gameObject.SetActive(true);
        CG_loadingScreen.alpha = 1;
    }

    private void FixedUpdate()
    {
        if (!_setup) return;
        foreach (var t in Transforms.value)
            UpdateTransform(t);
    }

    protected override void OnSpawned()
    {
        if (isController)
        {
            theme.value = SaveData.themeCurrent;
            lastLandingSpot.value = SaveData.lastLandingSpot;
            Setup();
            CreateNew();
        }
        base.OnSpawned();
    }
    protected override void OnDestroy()
    {
        Transforms.onChanged -= UpdateNetworkTransforms;
        theme.onChanged -= Setup;
        lastLandingSpot.onChanged -= Setup;
    }

    public int GetCollectedValue()
    {
        int _value = 0;
        foreach (var LG in List)
        {
            _value += LG.GetCollectedValue();
        }
        return _value;
    }

    [ObserversRpc]
    public static void LoadTheme(Themes.themeEnum _theme)
    {
        SaveData.themeCurrent = _theme;
        SaveData.i_currency += Instance.GetCollectedValue();
        SceneManager.LoadScene(1);
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
        LG.Setup(_transform.seed, List.Count);
        List.Add(LG);
        RequestTransform(List.Count - 1, NetworkManager.main.localPlayer);
    }
    public void CreateNew()
    {
        _networkTransform _transform = new _networkTransform();
        _transform.lgID = Transforms.value.Count;
        //_transform.UpdateTransform(Vector3.zero, Quaternion.identity);
        _transform.seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
        Transforms.value.Add(_transform);

        LevelGen LG = Instantiate(PF_levelGen, transform);
        LG.isHost = true;
        LG.Setup(_transform.seed, List.Count);
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
        foreach (var LG in List)
        {
            LG.AgentDeath(_AC);
        }
    }
    public void IsReady()
    {
        if (isReady)
            return;
        isReady = true;

        playerSpawner.canSpawn = true;
        onReadyEvent?.Invoke();
        StartCoroutine(FadeLoadingScreen());
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
}
