using System.Collections;
using TMPro;
using UnityEngine;

public class DayUI : MonoBehaviour
{
    public Animator A_anim;
    public TextMeshProUGUI TM_dayCounter;
    public GameObject G_morningHolder;
    public GameObject G_eveningHolder;
    public float F_visibleTime = 5;

    public void Display(int _dayCount)
    {
        A_anim.SetBool("Open", true);
        if (A_anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Close")
            A_anim.PlayClip("Open");

        bool _morning = _dayCount % 2 == 0;
        G_morningHolder.SetActive(_morning);
        G_eveningHolder.SetActive(!_morning);

        string _dayString = "DAY " + (1 + Mathf.RoundToInt(_dayCount / 2)).ToString();
        TM_dayCounter.text = _dayString;

        StartCoroutine(Display_Coroutine());
    }
    
    public void Hide()
    {
        A_anim.SetBool("Open", false);
        StopAllCoroutines();
    }
    IEnumerator Display_Coroutine()
    {
        yield return new WaitForSeconds(F_visibleTime);
        Hide();
    }
}
