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
    public List<stepClass> _steps = new List<stepClass>();
    [Space(10)]
    public objectiveClass _sideObjective = new objectiveClass();
    [System.Serializable]
    public class stepClass
    {
        public objectiveClass _objective;

        public stepClass Clone()
        {
            stepClass _temp = new stepClass();
            _temp._objective = _objective.Clone();
            return _temp;
        }
    }
    public Mission Clone()
    {
        Mission _temp = CreateInstance<Mission>();
        _temp._name = _name;
        _temp._description = _description;
        _temp._sprite = _sprite;

        _temp._steps = new List<stepClass>();
        foreach (var step in _steps)
            _temp._steps.Add(step.Clone());
        _temp._sideObjective = _sideObjective.Clone();

        return _temp;
    }
}
