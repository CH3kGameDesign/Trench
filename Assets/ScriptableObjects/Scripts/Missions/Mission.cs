using System.Collections.Generic;
using UnityEngine;
using static Objective;

[CreateAssetMenu(menuName = "Trench/Missions/Basic", fileName = "New Basic Mission")]
public class Mission : ScriptableObject
{
    [HideInInspector] public int _id;
    public string _name;
    public string _description;
    public Sprite _sprite;
    [Space(10)]
    public LevelGen PF_specificLayout;
    [Space(10)]
    public spawnOnStartEnum SpawnOnStart = (spawnOnStartEnum)~0;
    [System.Flags]
    public enum spawnOnStartEnum { 
        none = 0,
        companions = 1,
        enemies = 1 << 1,
        treasure = 1 << 2,
        trolley = 1 << 3,
    }
    [Space(10)]
    public List<stepClass> _steps = new List<stepClass>();
    [Space(10)]
    public objectiveClass _sideObjective = new objectiveClass();
    [System.Serializable]
    public class stepClass
    {
        public objectiveClass _objective;
        public spawnClass _spawnOnComplete;
        public stepClass Clone()
        {
            stepClass _temp = new stepClass();
            _temp._objective = _objective.Clone();
            _temp._spawnOnComplete = _spawnOnComplete.Clone();
            _temp.Setup();
            return _temp;
        }

        void Setup()
        {
            if (_spawnOnComplete._event != eventEnum.none)
                _objective.OnCompleted += _spawnOnComplete.SpawnEnemies_Check;
        }
    }
    public Mission Clone()
    {
        Mission _temp = CreateInstance<Mission>();
        _temp._id = _id;

        _temp._name = _name;
        _temp._description = _description;
        _temp._sprite = _sprite;

        _temp.PF_specificLayout = PF_specificLayout;

        _temp.SpawnOnStart = SpawnOnStart;

        _temp._steps = new List<stepClass>();

        foreach (var step in _steps)
            _temp._steps.Add(step.Clone());
        _temp._sideObjective = _sideObjective.Clone();

        _temp.SpawnEvents = new List<spawnClass>();
        foreach (var spawn in SpawnEvents)
            _temp.SpawnEvents.Add(spawn.Clone());

        return _temp;
    }

    public List<spawnClass> SpawnEvents = new List<spawnClass>();

    [System.Serializable]
    public class spawnClass
    {
        public eventEnum _event;
        public int _spawnAmt = -1;
        public float _delay = 0;
        [Space (10)]
        public LevelGen_Block.blockTypeEnum _blockType;
        public int _enemyRemainingAmt;

        [HideInInspector] public bool _activated = false;

        public spawnClass Clone()
        {
            spawnClass _temp = new spawnClass();
            _temp._event = _event;
            _temp._spawnAmt = _spawnAmt;
            _temp._delay = _delay;
            _temp._blockType = _blockType;
            _temp._enemyRemainingAmt = _enemyRemainingAmt;

            _temp._activated = _activated;

            return _temp;
        }

        public bool SpawnEnemies_Check(eventEnum _eventType,
            LevelGen_Block.blockTypeEnum? _type = null,
            int? _enemiesRemaining = null)
        {
            if (_activated) return false;
            if ((_event & _eventType) == 0) return false;
            switch (_event)
            {
                case eventEnum.playerEnterRoom:
                    if (_type.Value != _blockType)
                        return false;
                    break;
                case eventEnum.enemyRemaining:
                    if (_enemyRemainingAmt > _enemiesRemaining.Value)
                        return false;
                    break;
                default:
                    break;
            }
            SpawnEnemies();
            _activated = true;
            return true;
        }
        public void SpawnEnemies_Check()
        {
            if (_activated) return;
            if (_event == eventEnum.none) return;
            SpawnEnemies();
            _activated = true;
        }
        void SpawnEnemies()
        {
            List<LevelGen_Spawn> spawn = LevelGen_Holder.Instance.GetSpawns(_event, LevelGen_Spawn.spawnTypeEnum.enemy);
            
            if (_spawnAmt >= 0)
            {
                spawn.Shuffle();
                int _removeCount = spawn.Count - _spawnAmt;
                spawn.RemoveRange(_spawnAmt, _removeCount);
            }

            foreach (var item in spawn)
                item.Spawn();
        }
    }
    [System.Flags]
    public enum eventEnum {
        none = 0,
        levelLoaded = 1 << 0,
        objectiveComplete = 1 << 1,
        playerDiscovered = 1 << 2,
        playerEnterRoom = 1 << 3,
        enemyRemaining = 1 << 4
    }
    public void SpawnEnemies(eventEnum _event,
            LevelGen_Block.blockTypeEnum? _type = null,
            int? _enemiesRemaining = null)
    {
        foreach (var item in SpawnEvents)
            item.SpawnEnemies_Check(_event, _type, _enemiesRemaining);
    }

    public void OnLevelLoaded()
    {
        List<LevelGen_Spawn> spawn = new List<LevelGen_Spawn>();
        if (SpawnOnStart.HasFlag(spawnOnStartEnum.companions))
            spawn.AddRange(LevelGen_Holder.Instance.GetSpawns(
                eventEnum.levelLoaded, 
                LevelGen_Spawn.spawnTypeEnum.companion));
        if (SpawnOnStart.HasFlag(spawnOnStartEnum.enemies))
            spawn.AddRange(LevelGen_Holder.Instance.GetSpawns(
                eventEnum.levelLoaded,
                LevelGen_Spawn.spawnTypeEnum.enemy));
        if (SpawnOnStart.HasFlag(spawnOnStartEnum.treasure))
            spawn.AddRange(LevelGen_Holder.Instance.GetSpawns(
                eventEnum.levelLoaded,
                LevelGen_Spawn.spawnTypeEnum.treasure));
        if (SpawnOnStart.HasFlag(spawnOnStartEnum.trolley))
            spawn.AddRange(LevelGen_Holder.Instance.GetSpawns(
                eventEnum.levelLoaded,
                LevelGen_Spawn.spawnTypeEnum.shoppingCart));

        foreach (var item in spawn)
            item.Spawn();

        SpawnEnemies(eventEnum.levelLoaded);
    }
}
