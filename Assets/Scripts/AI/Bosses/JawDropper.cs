using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JawDropper : AgentController
{
    [Space(10)]
    public Slider S_hpSlider;
    [Space(10)]
    public AgentController PF_enemySpawn;
    private List<AgentController> activeChildren = new List<AgentController>();
    public float F_spawnTimer = 5;
    public int I_spawnLimit = 10;

    private Coroutine spawnEnemies = null;


    public override void AssignSlider(Slider _slider)
    {
        S_hpSlider = _slider;
        S_hpSlider.gameObject.SetActive(true);
        S_hpSlider.maxValue = F_maxHealth;
        S_hpSlider.minValue = 0;
        S_hpSlider.value = F_curHealth;
    }

    public override void OnHit(GunManager.bulletClass _bullet)
    {
        base.OnHit(_bullet);
        if (spawnEnemies == null)
            spawnEnemies = StartCoroutine(SpawnEnemies());
    }
    public override void HealthUpdate()
    {
        base.HealthUpdate();
        S_hpSlider.value = F_curHealth;
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            if (!b_alive)
                break;
            SpawnChild();
            yield return new WaitForSeconds(F_spawnTimer);
        }
    }

    void SpawnChild()
    {
        if (CheckChildren() < I_spawnLimit)
        {
            AgentController AC = Instantiate(PF_enemySpawn, NMA.transform.position, NMA.transform.rotation);
            AC.NMA.transform.rotation = NMA.transform.rotation;
            if (attackTarget.IsActive())
            {
                switch (attackTarget.targetType)
                {
                    case TargetClass.targetTypeEnum.player:
                        AC.AssignTarget_Attack(attackTarget.PC_tarPlayer);
                        break;
                    case TargetClass.targetTypeEnum.agent:
                        AC.AssignTarget_Attack(attackTarget.AC_tarAgent);
                        break;
                    default:
                        break;
                }
            }
            activeChildren.Add(AC);
            StartCoroutine(Toss(AC));
        }
    }

    IEnumerator Toss(AgentController AC)
    {
        yield return new WaitForEndOfFrame();
        AC.ChangeState(stateEnum.ragdoll);
        AC.StartCoroutine(AC.ChangeState(state, 3));
        Vector3 dir = NMA.transform.forward * 150;
        dir += Vector3.up;
        AC.RM_ragdoll.RB_rigidbodies[0].AddForce(dir, ForceMode.VelocityChange);
    }
    int CheckChildren()
    {
        int i = 0;
        for (int ii = activeChildren.Count - 1; ii >= 0; ii--)
        {
            if (activeChildren[ii].b_alive)
                i++;
            else
            {
                Destroy(activeChildren[ii].gameObject);
                activeChildren.RemoveAt(ii);
            }
        }
        return i;
    }
}
