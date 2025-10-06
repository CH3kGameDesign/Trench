using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using PurrNet;

public class EnemyTimer : NetworkBehaviour
{
    public static EnemyTimer Instance;
    [Range(10, 600)] public float F_timerLength_Boss = 300;
    [Range(10, 600)] public float F_timerLength_Ship = 30;
    private SyncVar<float> f_curTimer = new(0);
    private SyncVar<bool> b_active = new(false);
    private bool b_finished = false;
    public GameObject PF_bigBoss;
    public TextMeshProUGUI TM_timer;
    public GameObject G_timerBG;
    [HideInInspector] public Transform T_spawn_Boss;
    [HideInInspector] public LevelGen_Block LG_Ship;
    public GameObject PF_shipLandingPoint;
    private GameObject G_shipLandingPoint_Active;
    public Slider S_healthSlider;

    private bool timerActive = false;

    private void Awake()
    {
        Instance = this;
        G_timerBG.SetActive(false);
    }

    private void Update()
    {
        if (isServer || !b_active || b_finished) return;
        G_timerBG.SetActive(true);
        if (f_curTimer.value < 0)
        {
            TM_timer.text = "RUN";
            b_finished = true;
            return;
        }
        TM_timer.text = Mathf.FloorToInt(f_curTimer).ToString_Duration();
    }

    public void Setup(Transform _spawnBoss)
    {
        T_spawn_Boss = _spawnBoss;
    }

    [ServerRpc]
    public void StartTimer_Boss()
    {
        //Disabled Boss Timer
        return;
        if (!timerActive && SaveData.themeCurrent == Themes.themeEnum._default)
        {
            G_timerBG.SetActive(true);
            b_active.value = true;
            StartCoroutine(Timer_Boss());
        }
    }

    IEnumerator Timer_Boss()
    {
        timerActive = true;
        f_curTimer.value = F_timerLength_Boss;
        while (f_curTimer > 0)
        {
            TM_timer.text = Mathf.FloorToInt(f_curTimer).ToString_Duration();
            f_curTimer.value -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        TM_timer.text = "RUN";
        b_finished = true;
        SpawnBigBoss();
    }


    void SpawnBigBoss()
    {
        if (T_spawn_Boss != null)
        {
            GameObject GO = Instantiate(PF_bigBoss, T_spawn_Boss.transform.position, T_spawn_Boss.transform.rotation);
            AgentController AC = GO.GetComponent<AgentController>();
            AC.NMA.transform.rotation = T_spawn_Boss.transform.rotation;
        }
    }

    [ServerRpc]
    public void StartTimer_Ship()
    {
        if (!timerActive && SaveData.themeCurrent == Themes.themeEnum._default)
        {
            G_timerBG.SetActive(true);
            b_active.value = true;
            G_shipLandingPoint_Active = Instantiate(PF_shipLandingPoint, LG_Ship.transform.position, LG_Ship.transform.rotation);
            StartCoroutine(Timer_Ship());
        }
    }

    IEnumerator Timer_Ship()
    {
        timerActive = true;
        f_curTimer.value = F_timerLength_Ship;

        while (f_curTimer > 0)
        {
            TM_timer.text = Mathf.FloorToInt(f_curTimer).ToString_Duration();
            f_curTimer.value -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        TM_timer.text = "EXTRACT";
        b_finished = true;
        SpawnShip();
    }


    void SpawnShip()
    {
        if (LG_Ship != null)
        {
            LG_Ship.gameObject.SetActive(true);
            StartCoroutine(Move_Ship(3f));
        }
    }

    IEnumerator Move_Ship(float _duration)
    {
        float _timer = 0;
        Vector3 _startPos = LG_Ship.transform.position + (Vector3.up * 100);
        Vector3 _tarPos = LG_Ship.transform.position;

        while (_timer < 1)
        {
            LG_Ship.transform.position = Vector3.Lerp(_startPos, _tarPos, _timer);
            yield return new WaitForEndOfFrame();
            _timer += Time.deltaTime * _duration;
        }

        LG_Ship.transform.position = _tarPos;

        if (G_shipLandingPoint_Active)
            Destroy(G_shipLandingPoint_Active);
    }
}
