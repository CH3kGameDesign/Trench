using UnityEngine;
using System.Collections.Generic;

public class ObjectiveHUD : MonoBehaviour
{
    public List<ObjectiveHUDChild> Objectives = new List<ObjectiveHUDChild>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateObjectives()
    {
        int _amt = SaveData.objectives.Count;
        for (int i = 0; i < Objectives.Count; i++)
        {
            if (i < _amt)
                UpdateObjective(SaveData.objectives[i], Objectives[i]);
            else
                Objectives[i].gameObject.SetActive(false);
        }
    }    
    void UpdateObjective(Objective.objectiveClass _obj, ObjectiveHUDChild _child)
    {
        _child.gameObject.SetActive(true);
        _child.TM_description.text = _obj.GetString();
        _child.I_sprite.sprite = _obj.type.image;
    }
}
