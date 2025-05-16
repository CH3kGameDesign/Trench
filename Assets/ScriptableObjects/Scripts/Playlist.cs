using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static MusicHandler;

[CreateAssetMenu(menuName = "Trench/Music/Playlist", fileName = "New Playlist")]
public class Playlist : ScriptableObject
{
    public List<song> songList = new List<song>();
    public ambience ambienceList = new ambience();
    [System.Serializable]
    public class song
    {
        public AudioClip ambientLofi;
        public AudioClip drums;
        public AudioClip synth;
        public AudioClip guitar;
        public AudioClip bass;
        public AudioClip brass;
    }
    [System.Serializable]
    public class ambience
    {
        public List<AudioClip> ambientClutter = new List<AudioClip>();
        public List<AudioClip> ambientNoise = new List<AudioClip>();
        public List<AudioClip> heartbeat = new List<AudioClip>();
    }

    public List<song> GetSongList()
    {
        List<song> list = songList;
        list.Shuffle();
        return list;
    }

    public AudioClip GetAmbience(MusicHandler.typeEnum _type)
    {
        switch (_type)
        {
            case typeEnum.ambientClutter:   return ambienceList.ambientClutter.GetRandom();
            case typeEnum.ambientNoise:     return ambienceList.ambientNoise.GetRandom();
            case typeEnum.heartbeat:        return ambienceList.heartbeat.GetRandom();
            default: return null;
        }
    }
}
