using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ObjectiveHUD : MonoBehaviour
{
    public GameObject G_header;
    public TextMeshProUGUI TM_headerText;
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
            }
            else
                Objectives[i].gameObject.SetActive(false);
        }
        G_header.SetActive(_amt > 0);
    }    
    void UpdateObjective(Objective.objectiveClass _obj, ObjectiveHUDChild _child)
    {
        if (_obj.amt >= _obj.total && !_obj.completed)
        {
            CompleteObjective(_obj);
        }
        _child.gameObject.SetActive(true);
        _child.TM_description.text = _obj.GetDescription();
        _child.I_sprite.sprite = _obj.type.image;

        _child.TM_count.text = _obj.GetAmount();

        if (_obj.completed)
        {
            _child.I_background.color = new Color32(0xB0, 0xB0, 0xB0, 0xFF);
            _child.I_spriteBackground.color = new Color32(0xB0, 0xB0, 0xB0, 0xFF);
        }
        else
        {
            _child.I_background.color = new Color32(0x23, 0x23, 0x23, 0xFF);
            _child.I_spriteBackground.color = new Color32(0xFF, 0x95, 0x00, 0xFF);
        }
    }

    void CompleteObjective(Objective.objectiveClass _obj)
    {
        _obj.completed = true;
        PlayerManager.main.AH_agentAudioHolder.Play(AgentAudioHolder.type.objectiveComplete);
    }
}
