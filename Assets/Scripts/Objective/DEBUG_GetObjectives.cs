using UnityEngine;
using System.Collections.Generic;

public class DEBUG_GetObjectives : Interactable
{
    public Objective objectiveManager;
    public override void OnInteract(BaseController _player)
    {
        List<Objective.objectiveClass> _objectives = new List<Objective.objectiveClass>();
        for (int i = 0; i < 3; i++)
        {
            Objective.objectiveClass _temp = objectiveManager.GetObjective_Random();
            _objectives.Add(_temp);
        }
        SaveData.objectives = _objectives;
        _player.Update_Objectives();

        _player.AH_agentAudioHolder.Play(AgentAudioHolder.type.objectiveGain);
        base.OnInteract(_player);
    }
}
