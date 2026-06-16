using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Scripts")]
    public WaveManagement waveManagement;
    public MenuManager menuManagement;

    [Header("Audio Setup")]
    public AudioSource Audio;
    public AudioSource Click;
    public AudioFile[] AudioFile;

    private int level;
    private bool isSettingUp;
    private AudioType currentTrack = (AudioType)(-1);

    private readonly AudioType[] levelTracks = { 
        AudioType.Level1, 
        AudioType.Level2, 
        AudioType.Level3, 
        AudioType.Boss 
    };

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Click.Play();
        }
        if (waveManagement != null)
        {
            isSettingUp = waveManagement.isSettingUp;
            level = waveManagement.level;
        }

        AudioType targetTrack = GetRequiredTrack();

        if (targetTrack != currentTrack)
        {
            currentTrack = targetTrack;
            PlayMusic(currentTrack);
        }
    }

    private AudioType GetRequiredTrack()
    {
        if (SceneManager.GetSceneByName("Pause Menu").isLoaded) return AudioType.Menu;
        if (SceneManager.GetSceneByName("Win Menu").isLoaded) return AudioType.Win;
        if (SceneManager.GetSceneByName("Lose Menu").isLoaded) return AudioType.Lose;

        if (isSettingUp) return AudioType.SetUp;

        if (level <= 0) return AudioType.SetUp;

        int index = Mathf.Clamp(level - 1, 0, levelTracks.Length - 1);
        return levelTracks[index];
    }

    public void PlayMusic(AudioType type)
    {
        for (int i = 0; i < AudioFile.Length; i++)
        {
            if (AudioFile[i].type == type)
            {
                if (Audio.clip == AudioFile[i].audioClip && Audio.isPlaying) return;

                Audio.clip = AudioFile[i].audioClip;
                Audio.Play();
                break;
            }
        }
    }
}

[System.Serializable]
public struct AudioFile
{
    public AudioClip audioClip;
    public AudioType type;
}

public enum AudioType
{
    SetUp,
    Level1,
    Level2,
    Level3,
    Boss,
    Menu,
    Win,
    Lose
}