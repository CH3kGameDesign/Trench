using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using PurrNet;

public class EnemyTimer : NetworkBehaviour
{
    public static EnemyTimer Instance;
    [Range(60,600)]public float F_timerLength = 300;
    private SyncVar<float> f_curTimer = new(0);
    private SyncVar<bool> b_active = new(false);
    private bool b_finished = false;
    public GameObject PF_bigBoss;
    public TextMeshProUGUI TM_timer;
    public GameObject G_timerBG;
    [HideInInspector] public Transform T_spawn;
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

    public void Setup(Transform _spawn)
    {
        T_spawn = _spawn;
    }

    [ServerRpc]
    public void StartTimer()
    {
        if (!timerActive && SaveData.themeCurrent == Themes.themeEnum._default)
        {
            G_timerBG.SetActive(true);
            b_active.value = true;
            StartCoroutine(Timer());
        }
    }

    IEnumerator Timer()
    {
        timerActive = true;
        f_curTimer.value = F_timerLength;
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
        if (T_spawn != null)
        {
            GameObject GO = Instantiate(PF_bigBoss, T_spawn.transform.position, T_spawn.transform.rotation);
            AgentController AC = GO.GetComponent<AgentController>();
            AC.NMA.transform.rotation = T_spawn.transform.rotation;
        }
    }
}
