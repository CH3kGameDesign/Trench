using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JawDropper : AgentController
{
    public AgentController PF_enemySpawn;
    private List<AgentController> activeChildren = new List<AgentController>();
    public float F_spawnTimer = 5;
    public int I_spawnLimit = 10;


    public override void Start()
    {
        base.Start();
        StartCoroutine(SpawnEnemies());
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
