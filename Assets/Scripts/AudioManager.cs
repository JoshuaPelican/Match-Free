using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static AudioSource SFXSource;

    private void Awake()
    {
        SFXSource = GetComponent<AudioSource>();
    }

    public static void PlayEffect(AudioClip clip, float volume, float pitch, float volumeVariance = 0.1f, float pitchVariance = 0.1f)
    {
        SFXSource.clip = clip;
        SFXSource.volume = volume + Random.Range(-volumeVariance, volumeVariance);
        SFXSource.pitch = pitch + Random.Range(-pitchVariance, pitchVariance);
        SFXSource.Play();
    }
}
