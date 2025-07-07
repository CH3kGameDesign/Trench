using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class EnemyTimer : MonoBehaviour
{
    public static EnemyTimer Instance;
    [Range(60,600)]public float F_timerLength = 300;
    private float f_curTimer = 0;
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

    public void Setup(Transform _spawn)
    {
        T_spawn = _spawn;
    }

    public void StartTimer()
    {
        if (!timerActive && SaveData.themeCurrent == Themes.themeEnum._default)
        {
            G_timerBG.SetActive(true);
            StartCoroutine(Timer());
        }
    }

    IEnumerator Timer()
    {
        timerActive = true;
        f_curTimer = F_timerLength;
        while (f_curTimer > 0)
        {
            TM_timer.text = Mathf.FloorToInt(f_curTimer).ToString_Duration();
            f_curTimer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        TM_timer.text = "RUN";
        SpawnBigBoss();
    }
    void SpawnBigBoss()
    {
        if (T_spawn != null)
        {
            GameObject GO = Instantiate(PF_bigBoss, T_spawn.transform.position, T_spawn.transform.rotation);
            AgentController AC = GO.GetComponent<AgentController>();
            AC.NMA.transform.rotation = T_spawn.transform.rotation;
            AC.AssignSlider(S_healthSlider);
        }
    }
}
