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

    /// <summary>
    /// Retuns oneshot audioclip by searching from the list 
    /// </summary>
    /// <param name="_id">ID number you give to the sound in inspector </param>
    /// <returns></returns>
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

    /// <summary>
    /// Plays oneshot audioclip by searching from the list 
    /// </summary>
    /// <param name="_id"></param>
    public static void PlayOneShotAudio(string _id)
    {
        AudioClip clip=instance.GetOneShotAudio(_id);
        instance.mainSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Profile class for associating audio files with identification number 
    /// </summary>
    [System.Serializable]
    public class AudioProfile
    {
        public string id;
        public AudioClip audioClip;
    }
}
