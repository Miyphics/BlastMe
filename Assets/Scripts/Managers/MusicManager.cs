using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private bool musicLoop;
    [SerializeField] private List<AudioClip> deepHouseTracks;

    private const string MIXER_MUSIC = "MusicVolume";

    private List<AudioClip> musicQueue;

    private AudioSource musicSource;
    private bool canPlayMusic = true;
    public bool CanPlayMusic { get { return canPlayMusic; }
        set
        {
            canPlayMusic = value;

            musicQueue?.Clear();

            if (value)
                PlayMusic();
            else
                StopMusic();
        }
    }

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
    }

    public void StartPlayingMusic()
    {
        StartCoroutine(CheckAudioIsPlayingIE());
    }

    private IEnumerator CheckAudioIsPlayingIE()
    {
        while (true)
        {
            if (musicLoop && canPlayMusic)
            {
                if ((musicSource.clip == null || !musicSource.isPlaying) && Application.isFocused)
                {
                    PlayMusicFromQueue();
                }
            }

            yield return new WaitForSeconds(5f);
        }
    }

    private void PlayMusicFromQueue()
    {
        if (musicQueue == null || musicQueue.Count < 1)
        {
            CreateMusicQueue();
        }

        musicSource.Stop();
        musicSource.clip = musicQueue[^1];
        musicSource.Play();
        GameManager.Instance.HudManager.ShowCurrentMusicName(musicSource.clip.name);

        musicQueue.RemoveAt(musicQueue.Count - 1);
    }

    private void CreateMusicQueue()
    {
        musicQueue = new List<AudioClip>();
        musicQueue.AddRange(deepHouseTracks);

        musicQueue.Shuffle();
    }

    public void StopMusic()
    {
        canPlayMusic = false;
        musicSource.Stop();
    }

    public void PlayMusic()
    {
        canPlayMusic = true;
        PlayMusicFromQueue();
    }

    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f);
        musicSource.outputAudioMixerGroup.audioMixer.SetFloat(MIXER_MUSIC, Mathf.Log10(volume) * 20f);
    }

    public float GetVolume()
    {
        musicSource.outputAudioMixerGroup.audioMixer.GetFloat(MIXER_MUSIC, out float volume);
        if (volume == 0f) return 1f;

        volume = Mathf.Pow(10, volume / 20f);
        return volume;
    }
}
