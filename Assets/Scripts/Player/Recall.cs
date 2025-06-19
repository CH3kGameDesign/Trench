using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Recall : MonoBehaviour
{
    public float F_delay = 3f;
    public float F_coolDown = 3f;
    public Animator A_circle;
    public Image I_circle;
    public TextMeshProUGUI TM_input;

    private float f_timer = 0f;
    private float f_timerCoolDown = 0f;

    private Transform t_recallPos;

    private bool _active = false;

    PlayerController PC;
    public void Setup(PlayerController _PC)
    {
        PC = _PC;
        f_timer = 0;
        f_timerCoolDown = F_coolDown;
        I_circle.gameObject.SetActive(false);
    }

    public void SetRecallPos(Transform _t)
    {
        t_recallPos = _t;
    }

    public void TextUpdate()
    {
        TM_input.text = "".ToString_Input(PlayerController.inputActions.Recall, TM_input, Interactable.enumType.input);
    }

    public void Update()
    {
        if (f_timerCoolDown > 0)
            f_timerCoolDown -= Time.deltaTime;
    }

    public void _Update()
    {
        if (f_timerCoolDown > 0)
            return;
        if (PC.Inputs.b_recall)
            f_timer += Time.deltaTime / F_delay;
        else
            f_timer -= Time.deltaTime / F_delay;
        f_timer = Mathf.Clamp01(f_timer);

        if (f_timer <= 0f)
        {
            if (_active)
            {
                A_circle.SetBool("Open", false);
                _active = false;
            }
        }
        else
        {
            if (!_active)
            {
                A_circle.SetBool("Open", true);
                _active = true;
            }
            I_circle.fillAmount = f_timer;
            if (f_timer >= 1f)
                Activate();
        }
    }

    void Activate()
    {
        f_timer = 0;
        f_timerCoolDown = F_coolDown;
        A_circle.SetBool("Open", false);

        if (t_recallPos != null)
        {
            PC.NMA.transform.rotation = t_recallPos.rotation;
            PC.NMA.Warp(t_recallPos.position);
        }
    }
}
