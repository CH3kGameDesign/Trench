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
            {
                UpdateObjective(SaveData.objectives[i], Objectives[i]);
                if (SaveData.objectives[i].amt >= SaveData.objectives[i].total)
                {
                    //CompleteObjective(SaveData.objectives[i]);
                    //return;
                }
            }
            else
                Objectives[i].gameObject.SetActive(false);
        }
    }    
    void UpdateObjective(Objective.objectiveClass _obj, ObjectiveHUDChild _child)
    {
        _child.gameObject.SetActive(true);
        _child.TM_description.text = _obj.GetString();
        _child.I_sprite.sprite = _obj.type.image;

        if (_obj.amt >= _obj.total)
        {
            _child.I_background.color = new Color(0.3f, 0.8f, 0.3f);
        }
        else
        {
            _child.I_background.color = new Color(1f, 1f, 1f);
        }
    }

    void CompleteObjective(Objective.objectiveClass _obj)
    {
        SaveData.objectives.Remove(_obj);
        UpdateObjectives();
    }
}
