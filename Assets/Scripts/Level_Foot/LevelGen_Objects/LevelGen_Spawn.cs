using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Mission;

public class LevelGen_Spawn : LevelGen_Object
{
    public spawnTypeEnum spawnType = spawnTypeEnum.enemy;
    public enum spawnTypeEnum { player, companion, enemy, treasure, boss, shoppingCart };
    public AgentController.stateEnum _state = AgentController.stateEnum.unchanged;
    public GameObject PF_override;
    [Space(10)]
    public eventEnum spawnEvent = eventEnum.levelLoaded;
    public bool singleUse = true;
    private bool used = false;
    [Space(10)]
    public Color C_Color = new Color (1,0,0,0.4f);

    private List<AgentController> _spawnedAgents = new List<AgentController>();

    public GameObject G_sceneViewObject;
    private LevelGen LG;
    private LevelGen_Theme LG_Theme;

    // Start is called before the first frame update
    void Start()
    {
        ShowInGameView(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 2);
    }
    public void Setup(LevelGen _LG)
    {
        LG = _LG;
        LG_Theme = LG.LG_Theme;
    }
    void ShowInGameView(bool _show)
    {
        G_sceneViewObject.SetActive(_show);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = C_Color;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        Vector3 drawBoxVector = new Vector3(0.5f, 0.2f, 0.5f);
        Vector3 drawBoxPosition = transform.position + (transform.forward / 2.5f);

        Gizmos.matrix = Matrix4x4.TRS(drawBoxPosition, transform.rotation * Quaternion.Euler(0, 45, 0), drawBoxVector);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
    public bool Spawn(
        LevelGen.spawnType _spawnType = LevelGen.spawnType._default)
    {
        if (LG == null)
            return false;
        if (!LG.isHost)
            return false;
        if (singleUse && used)
            return false;

        GameObject prefab;
        GameObject GO;

        Unity.Mathematics.Random Random_Seeded = new Unity.Mathematics.Random((uint)1011);

        switch (spawnType)
        {
            case LevelGen_Spawn.spawnTypeEnum.companion:
                if (_spawnType == LevelGen.spawnType.enemyOnly)
                    return false;
                if (PF_override != null) prefab = PF_override;
                else prefab = LG_Theme.GetCompanion(Random_Seeded);

                Random_Seeded.NextInt();
                if (prefab != null)
                {
                    GO = Instantiate(prefab);
                    AgentController AC = GO.GetComponent<AgentController>();
                    AC.NMA.transform.position = transform.position;
                    AC.NMA.transform.rotation = transform.rotation;
                    AC.ChangeState(_state);
                    LG.AC_agents.Add(AC);
                    _spawnedAgents.Add(AC);
                }
                break;
            case LevelGen_Spawn.spawnTypeEnum.enemy:
                if (_spawnType == LevelGen.spawnType.friendlyOnly)
                    return false;
                if (PF_override != null) prefab = PF_override;
                else prefab = LG_Theme.GetEnemy(Random_Seeded);

                Random_Seeded.NextInt();
                if (prefab != null)
                {
                    GO = Instantiate(prefab);
                    AgentController AC = GO.GetComponent<AgentController>();
                    AC.NMA.transform.position = transform.position;
                    AC.NMA.transform.rotation = transform.rotation;
                    AC.ChangeState(_state);
                    LG.AC_agents.Add(AC);
                    _spawnedAgents.Add(AC);
                }
                break;
            case LevelGen_Spawn.spawnTypeEnum.treasure:
                if (_spawnType == LevelGen.spawnType.friendlyOnly)
                    return false;
                if (PF_override != null) prefab = PF_override;
                else prefab = LG_Theme.GetTreasure(Random_Seeded);

                Random_Seeded.NextInt();
                if (prefab != null)
                    GO = Instantiate(prefab, transform.position, transform.rotation, LG.transform);
                break;
            case LevelGen_Spawn.spawnTypeEnum.shoppingCart:
                if (_spawnType == LevelGen.spawnType.friendlyOnly)
                    return false;
                if (PF_override != null) prefab = PF_override;
                else prefab = null;

                if (prefab != null)
                    GO = Instantiate(prefab, transform.position, transform.rotation, LG.transform);
                break;
            default:
                break;
        }

        used = true;
        return true;
    }

    public void Agent_SetProtect()
    {
        foreach (var item in _spawnedAgents)
        {
            item.ChangeState(AgentController.stateEnum.protect);
        }
    }
}
