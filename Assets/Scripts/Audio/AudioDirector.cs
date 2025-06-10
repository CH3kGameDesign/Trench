using UnityEngine;

public class AudioDirector : MonoBehaviour
{
    public AgentAudioHolder AudioHolder;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Step_Walk() { if (AudioHolder != null) AudioHolder.Play(AgentAudioHolder.type.step); }
    public void Step_Crouch() { if (AudioHolder != null) AudioHolder.Play(AgentAudioHolder.type.crouchStep); }
    public void Step_Sprint() { if (AudioHolder != null) AudioHolder.Play(AgentAudioHolder.type.sprintStep); }
}
