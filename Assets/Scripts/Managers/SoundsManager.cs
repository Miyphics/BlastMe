using System.Collections.Generic;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    [SerializeField] private List<AudioClip> ballonsPops;
    [SerializeField] private List<AudioClip> bombExplode;
    [SerializeField] private List<AudioClip> buttonClick;
    [SerializeField] private List<AudioClip> coinPickup;

    [SerializeField] private AudioSource ballonsAudio;

    public AudioClip GetRandomBallonPops()
    {
        return ballonsPops[Random.Range(0, ballonsPops.Count)];
    }

    public AudioClip GetRandomBombExplode()
    {
        return bombExplode[Random.Range(0, bombExplode.Count)];
    }

    public AudioClip GetRandomCoinPickup()
    {
        return coinPickup[Random.Range(0, coinPickup.Count)];
    }

    public void SetupAudioPops(AudioSource source)
    {
        source.pitch = Random.Range(0.85f, 1.1f);
        source.volume = Random.Range(0.9f, 1f);
        source.clip = GetRandomBallonPops();
    }

    public void SetupAudioBomb(AudioSource source)
    {
        source.pitch = Random.Range(0.9f, 1.05f);
        source.volume = Random.Range(0.9f, 1f);
        source.clip = GetRandomBombExplode();
    }

    public void PlayCoinPickup()
    {
        SetupAudioCoinPickup(ballonsAudio);

        if (!ballonsAudio.isPlaying)
            ballonsAudio.Play();
    }

    public void SetupAudioCoinPickup(AudioSource source)
    {
        source.pitch = Random.Range(0.85f, 1.05f);
        source.volume = Random.Range(0.9f, 1f);
        source.clip = GetRandomCoinPickup();
    }
}
