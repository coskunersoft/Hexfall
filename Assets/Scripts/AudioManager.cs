using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private AudioSource mainSource;
    public List<AudioProfile> oneShotProfiles;
    private Dictionary<string,AudioClip> temp;

    private void Awake()
    {
        mainSource = GetComponent<AudioSource>();
        instance = this;
        temp = new Dictionary<string, AudioClip>();
    }

    private AudioClip GetOneShotAudio(string _id)
    {
        AudioClip clip = null;
        if (temp.ContainsKey(_id))
        {
            clip = temp[_id];
        }
        else
        {
            clip = oneShotProfiles.Find(x => x.id == _id).audioClip;
            temp.Add(_id, clip);
            if (temp.Count>10)
            {
                temp.Remove(temp.First().Key);
            }
        }
        return clip;
    }
    public static void PlayOneShotAudio(string _id)
    {
        AudioClip clip=instance.GetOneShotAudio(_id);
        instance.mainSource.PlayOneShot(clip);
    }

    [System.Serializable]
    public class AudioProfile
    {
        public string id;
        public AudioClip audioClip;
    }
}
