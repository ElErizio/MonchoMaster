using UnityEngine;
using System.Collections;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private SoundDatabase soundDatabase;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySFX(string soundName)
    {
        Sound sound = soundDatabase.sounds.FirstOrDefault(s => s.name == soundName);
        if (sound != null)
        {
            sfxSource.PlayOneShot(sound.clip);
        }
        else
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
        }
    }

    public void PlayMusic(string musicName)
    {
        Sound music = soundDatabase.sounds.FirstOrDefault(s => s.name == musicName);
        if (music != null)
        {
            musicSource.clip = music.clip;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Music: " + musicName + " not found!");
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}