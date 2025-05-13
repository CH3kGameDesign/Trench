using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using static UnityEditor.ShaderData;
using UnityEngine.Audio;

public class MusicHandler : MonoBehaviour
{
    public static MusicHandler Instance;
    public segments Segments = new segments();
    public misc Misc = new misc();
    [Space(10)]
    public AudioMixer MixerGroup;
    public AudioMixerSnapshot[] MixerSnapshots;
    public enum typeEnum { 
        ambientLofi, drums, synth, guitar, bass, brass, 
        ambientClutter, ambientNoise, heartbeat
    }

    [System.Serializable]
    public class segments
    {
        public AudioCon ambientLofi;
        public AudioCon drums;
        public AudioCon synth;
        public AudioCon guitar;
        public AudioCon bass;
        public AudioCon brass;

        public void Play()
        {
            ambientLofi.Play();
            drums.Play();
            synth.Play();
            guitar.Play();
            bass.Play();
            brass.Play();
        }
        public void Stop()
        {
            ambientLofi.Stop();
            drums.Stop();
            synth.Stop();
            guitar.Stop();
            bass.Stop();
            brass.Stop();
        }
        public void ReduceCheck()
        {
            ambientLofi.ReduceCheck();
            drums.ReduceCheck();
            synth.ReduceCheck();
            guitar.ReduceCheck();
            bass.ReduceCheck();
            brass.ReduceCheck();
        }
    }
    [System.Serializable]
    public class misc
    {
        public AudioCon ambientClutter;
        public AudioCon ambientNoise;
        public AudioCon heartbeat;

        public void Play()
        {
            ambientClutter.Play();
            ambientNoise.Play();
            heartbeat.Play();
        }
        public void Stop()
        {
            ambientClutter.Stop();
            ambientNoise.Stop();
            heartbeat.Stop();
        }
        public void ReduceCheck()
        {
            ambientClutter.ReduceCheck();
            ambientNoise.ReduceCheck();
            heartbeat.ReduceCheck();
        }
    }

    [System.Serializable]
    public class AudioCon
    {
        public AudioSource source;
        public float reduceDelay = -1;
        private float reduceTimer = 0;
        public float volume { get; private set; }
        public float tarVolume { get; private set; }
        private Coroutine coroutine;

        public void Play() { source.Play(); }
        public void Stop() { source.Stop(); }
        public void ReduceCheck()
        {
            if (reduceDelay > 0 && volume > 0)
            {
                reduceTimer += Time.deltaTime;
                if (reduceTimer > reduceDelay)
                    SetVolume(0, 4);
            }
        }
        void SetVolume(float _volume)
        {
            tarVolume = Mathf.Clamp01(_volume);
            volume = tarVolume;
            source.volume = volume;
            reduceTimer = 0;
        }
        public void SetVolume(float _volume, float _speed)
        {
            if (coroutine != null)
                MusicHandler.Instance.StopCoroutine(coroutine);
            if (_speed <= 0)
                SetVolume(_volume);
            else
                coroutine = MusicHandler.Instance.StartCoroutine(ChangeVolume_Coroutine(_volume, _speed));
        }

        IEnumerator ChangeVolume_Coroutine(float _tarVolume, float _speed)
        {
            float _timer = 0;
            float _startVol = volume;
            tarVolume = _tarVolume;
            if (!source.isPlaying)
                Play();
            while (_timer < 1)
            {
                SetVolume(Mathf.Lerp(_startVol, _tarVolume, _timer));
                yield return new WaitForEndOfFrame();
                _timer += Time.deltaTime;
            }
            SetVolume(_tarVolume);
        }
    }
    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetStartVolumes();
        Segments.Play();
    }

    void SetStartVolumes()
    {
        Segments.ambientLofi.SetVolume(1, 0);
        Segments.drums.SetVolume(0, 0);
        Segments.synth.SetVolume(0, 0);
        Segments.guitar.SetVolume(0, 0);
        Segments.bass.SetVolume(0, 0);
        Segments.brass.SetVolume(0, 0);

        Misc.ambientClutter.SetVolume(0, 0);
        Misc.ambientNoise.SetVolume(0, 0);
        Misc.heartbeat.SetVolume(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Segments.ReduceCheck();
        Misc.ReduceCheck();
    }

    public static void SetVolume(typeEnum _type, float _volume, float _speed = 0.5f)
    {
        if (Instance == null)
            return;
        AudioCon _temp = Instance.GetType(_type);
        _temp.SetVolume(Mathf.Clamp01(_volume), _speed);
    }
    public static void AdjustVolume(typeEnum _type, float _volume, float _speed = 0.5f)
    {
        if (Instance == null)
            return;
        AudioCon _temp = Instance.GetType(_type);
        _temp.SetVolume(Mathf.Clamp01(_volume + _temp.tarVolume), _speed);
    }
    public static void Muffle(bool _on = true)
    {
        if (Instance == null)
            return;
        float[] weights = { 1f, 0f, 0f};
        if (_on)
        {
            weights[0] = 0f;
            weights[1] = 1f;
        }
        Instance.MixerGroup.TransitionToSnapshots(Instance.MixerSnapshots, weights, 0.5f);
    }
    AudioCon GetType(typeEnum _type)
    {
        switch (_type)
        {
            case typeEnum.ambientLofi: return Segments.ambientLofi;
            case typeEnum.drums: return Segments.drums;
            case typeEnum.synth: return Segments.synth;
            case typeEnum.guitar: return Segments.guitar;
            case typeEnum.bass: return Segments.bass;
            case typeEnum.brass: return Segments.brass;

            case typeEnum.ambientClutter: return Misc.ambientClutter;
            case typeEnum.ambientNoise: return Misc.ambientNoise;
            case typeEnum.heartbeat: return Misc.heartbeat;
            default: return null;
        }
    }
}
