using System;
using UnityEngine;

public class EnemySFX : MonoBehaviour 
{
    [Header("Audio Setup")]
    public AudioSource Audio;
    public AudioSfx[] AudioSfxList;

    public void DeathSFX() 
    { 
        PlaySFX(SfxType.Death, 0.1f); 
    }

    public void HitSFX() 
    { 
        PlaySFX(SfxType.Hit, 0.5f); 
    }

    public void BlinkSFX() 
    { 
        PlaySFX(SfxType.Blink, 0.5f); 
    }

    public void PlaySFX(SfxType type, float volume) 
    {

        if (Audio == null) 
        {
            Audio = GetComponent<AudioSource>();
        }

        for (int i = 0; i < AudioSfxList.Length; i++) 
        {
            if (AudioSfxList[i].type == type) 
            {
                if (AudioSfxList[i].audioClip != null) 
                {
                    Audio.PlayOneShot(AudioSfxList[i].audioClip, volume);
                } 
                break; 
            }
        }
    }
}

[System.Serializable]
public struct AudioSfx 
{
    public AudioClip audioClip;
    public SfxType type;
}

public enum SfxType 
{
    Death,
    Hit,
    Blink
}