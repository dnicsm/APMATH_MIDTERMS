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

    public void ShadowSFX()
    {
        PlaySFX(SfxType.Shadow, 0.5f);
    }

    public void SmokeSFX()
    {
        PlaySFX(SfxType.Smoke, 0.6f);
    }

    public void CloneSFX()
    {
        PlaySFX(SfxType.Clone, 0.5f);
    }

    public void BossPhaseSFX()
    {
        PlaySFX(SfxType.BossPhase, 0.8f);
    }

    public void DebuffSFX()
    {
        PlaySFX(SfxType.Debuff, 0.5f);
    }

    public void PlaySFX(SfxType type, float volume)
    {
        if (Audio == null)
        {
            Audio = GetComponent<AudioSource>();
        }

        if (Audio == null || AudioSfxList == null) return;

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
    Shadow,
    Smoke,
    Clone,
    BossPhase,
    Debuff
}