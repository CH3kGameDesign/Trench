using UnityEngine;
using System.Collections;

public class AgentAudioHolder : MonoBehaviour
{
    public audioSourceClass sources = new audioSourceClass();
    public audioClipClass clips = new audioClipClass();
    [System.Serializable]
    public class audioSourceClass
    {
        public AudioSource A_hurt;
        public AudioSource A_death;
        [Space(10)]
        public AudioSource A_jump;
        public AudioSource A_land;
        public AudioSource A_stand;
        public AudioSource A_crouch;
        public AudioSource A_sprint;
        [Space(10)]
        public AudioSource A_step;
        public AudioSource A_sprintStep;
        public AudioSource A_crouchStep;
        [Space(10)]
        public AudioSource A_pickupSmall;
        public AudioSource A_pickup;
        public AudioSource A_throww;
        [Space(10)]
        public AudioSource A_fire;
        public AudioSource A_melee;
        public AudioSource A_reload;
        public AudioSource A_equip;
        [Space(10)]
        public AudioSource A_radial;
        public AudioSource A_radialTick;
        public AudioSource A_radialSubTick;
        [Space(10)]
        public AudioSource A_objectiveGain;
        public AudioSource A_objectiveTick;
        public AudioSource A_objectiveComplete;
    }
    [System.Serializable]
    public class audioClipClass
    {
        public AudioClip[] hurt = new AudioClip[0];
        public AudioClip[] death = new AudioClip[0];
        [Space(10)]
        public AudioClip[] jump = new AudioClip[0];
        public AudioClip[] land = new AudioClip[0];
        public AudioClip[] stand = new AudioClip[0];
        public AudioClip[] crouch = new AudioClip[0];
        public AudioClip[] sprint = new AudioClip[0];
        [Space(10)]
        public AudioClip[] step = new AudioClip[0];
        public AudioClip[] sprintStep = new AudioClip[0];
        public AudioClip[] crouchStep = new AudioClip[0];
        [Space(10)]
        public AudioClip[] pickupSmall = new AudioClip[0];
        public AudioClip[] pickup = new AudioClip[0];
        public AudioClip[] throww = new AudioClip[0];
        [Space(10)]
        public AudioClip[] fire = new AudioClip[0];
        public AudioClip[] melee = new AudioClip[0];
        public AudioClip[] reload = new AudioClip[0];
        public AudioClip[] equip = new AudioClip[0];
        [Space(10)]
        public AudioClip[] radial = new AudioClip[0];
        public AudioClip[] radialTick = new AudioClip[0];
        public AudioClip[] radialSubTick = new AudioClip[0];
        [Space(10)]
        public AudioClip[] objectiveGain = new AudioClip[0];
        public AudioClip[] objectiveTick = new AudioClip[0];
        public AudioClip[] objectiveComplete = new AudioClip[0];
    }

    public enum type
    {
        hurt,
        death,

        jump,
        land,
        stand,
        crouch,
        sprint,

        step,
        sprintStep,
        crouchStep,

        pickupSmall,
        pickup,
        throww,

        fire,
        melee,
        reload,
        equip,

        radial,
        radialTick,
        radialSubTick,

        objectiveGain,
        objectiveTick,
        objectiveComplete,
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Gun_Equip(GunClass _gun)
    {
        clips.fire = _gun.audioClips.fire;
        clips.melee = _gun.audioClips.melee;
        clips.reload = _gun.audioClips.reload;
        clips.equip = _gun.audioClips.equip;
    }

    public void Play(type _type)
    {
        switch (_type)
        {
            case type.hurt: 
                Play(sources.A_hurt, clips.hurt); break;
            case type.death: 
                Play(sources.A_death, clips.death); break;

            case type.jump: 
                Play(sources.A_jump, clips.jump); break;
            case type.land:
                Play(sources.A_land, clips.land); break;
            case type.stand:
                Play(sources.A_stand, clips.stand); break;
            case type.crouch:
                Play(sources.A_crouch, clips.crouch); break;
            case type.sprint:
                if (!sources.A_sprint.isPlaying)
                    StartCoroutine(FadeIn(sources.A_sprint, clips.sprint)); break;

            case type.step: 
                Play(sources.A_step, clips.step); break;
            case type.sprintStep:
                Play(sources.A_sprintStep, clips.sprintStep); break;
            case type.crouchStep:
                Play(sources.A_crouchStep, clips.crouchStep); break;

            case type.pickupSmall:
                Play(sources.A_pickupSmall, clips.pickupSmall); break;
            case type.pickup:
                Play(sources.A_pickup, clips.pickup); break;
            case type.throww:
                Play(sources.A_throww, clips.throww); break;

            case type.fire:
                Play(sources.A_fire, clips.fire); break;
            case type.melee:
                Play(sources.A_melee, clips.melee); break;
            case type.reload:
                Play(sources.A_reload, clips.reload); break;
            case type.equip:
                Play(sources.A_equip, clips.equip); break;

            case type.radial:
                if (!sources.A_radial.isPlaying)
                    StartCoroutine(FadeIn(sources.A_radial, clips.radial, 0.5f, true)); break;
            case type.radialTick:
                Play(sources.A_radialTick, clips.radialTick); break;
            case type.radialSubTick:
                Play(sources.A_radialSubTick, clips.radialSubTick); break;

            case type.objectiveGain:
                Play(sources.A_objectiveGain, clips.objectiveGain); break;
            case type.objectiveTick:
                Play(sources.A_objectiveTick, clips.objectiveTick); break;
            case type.objectiveComplete:
                Play(sources.A_objectiveComplete, clips.objectiveComplete); break;
            default:
                break;
        }
    }
    public void Stop(type _type)
    {
        switch (_type)
        {
            case type.sprint:
                if (sources.A_sprint.isPlaying)
                    StartCoroutine(FadeOut(sources.A_sprint)); 
                break;

            case type.radial:
                if (sources.A_radial.isPlaying)
                    StartCoroutine(FadeOut(sources.A_radial, 0.5f, true));
                break;
            default:
                break;
        }
    }

    void Play(AudioSource _source, AudioClip[] _clips)
    {
        if (_clips.Length > 0)
        {
            int i = Random.Range(0, _clips.Length);
            _source.clip = _clips[i];
            _source.pitch = 1 + Random.Range(-0.1f, 0.1f);
            _source.Play();
        }
    }

    IEnumerator FadeIn(AudioSource _source, AudioClip[] _clips, float _speed = 0.5f, bool _unscaled = false)
    {
        _source.volume = 0;
        Play(_source, _clips);
        float timer = 0;
        while (timer < 1)
        {
            _source.volume = Mathf.Lerp(0, 1, timer);
            yield return new WaitForEndOfFrame();
            if (_unscaled)
                timer += Time.unscaledDeltaTime / _speed;
            else
                timer += Time.deltaTime / _speed;
        }
        _source.volume = 1;
    }
    IEnumerator FadeOut(AudioSource _source, float _speed = 0.5f, bool _unscaled = false)
    {
        float _volume = _source.volume;
        float timer = 0;
        while (timer < 1)
        {
            _source.volume = Mathf.Lerp(_volume, 0, timer);
            yield return new WaitForEndOfFrame();
            if (_unscaled)
                timer += Time.unscaledDeltaTime / _speed;
            else
                timer += Time.deltaTime / _speed;
        }
        _source.Stop();
        _source.volume = _volume;
    }
}