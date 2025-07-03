using UnityEngine;
using TMPro;
using System.Collections;

public class EnemyTimer : MonoBehaviour
{
    public static EnemyTimer Instance;
    [Range(60,600)]public float F_timerLength = 300;
    private float f_curTimer = 0;
    public GameObject PF_bigBoss;
    public TextMeshProUGUI TM_timer;
    [HideInInspector] public Transform T_spawn;

    private bool timerActive = false;

    private void Awake()
    {
        Instance = this;
    }

    public void Setup(Transform _spawn)
    {
        T_spawn = _spawn;
    }

    public void StartTimer()
    {
        if (!timerActive && SaveData.themeCurrent == Themes.themeEnum._default)
            StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        timerActive = true;
        f_curTimer = F_timerLength;
        while (f_curTimer > 0)
        {
            TM_timer.text = Mathf.Floor(f_curTimer).ToString();
            f_curTimer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        SpawnBigBoss();
    }
    void SpawnBigBoss()
    {
        GameObject GO = Instantiate(PF_bigBoss);
        GO.transform.position = T_spawn.position;
    }
}
