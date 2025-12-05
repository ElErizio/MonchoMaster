using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private SoundDatabase soundDatabase;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    private float sfxVolumeBeforeMute = 1f;
    private float musicVolumeBeforeMute = 1f;


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
        if (soundDatabase == null)
        {
            Debug.LogError("SoundDatabase no asignado en AudioManager");
            return;
        }

        AudioClip clip = soundDatabase.GetAudioClip(soundName);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic(string musicName)
    {
        if (soundDatabase == null)
        {
            Debug.LogError("SoundDatabase no asignado en AudioManager");
            return;
        }

        AudioClip clip = soundDatabase.GetAudioClip(musicName);
        if (clip != null)
        {
            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.clip = clip;
            musicSource.Play();
        }
    }


    private bool sfxToggle = false;

    public void SFXVolume()
    {
        AudioManager.Instance.ClickSelect();

        if (sfxToggle)
        {
            AudioManager.Instance.SetSFXVolume(1);
        }
        else
        {
            AudioManager.Instance.SetSFXVolume(0);
        }

        sfxToggle = !sfxToggle;
    }


    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    public void CreditsMusic()
    {
        PlayMusic("CreditsMusic");
    }

    public void MainMenuMusic()
    {
        PlayMusic("MainMenuMusic");
    }
    public void MusicPuesto()
    {
        PlayMusic("MusicPuesto");
    }

    public void MonchoMaster()
    {
        PlayMusic("Moncho Master");
    }

    public void LoteriaComplete()
    {
        PlaySFX("LoteriaComplete");
    }
    public void BuildCartas()
    {
        PlaySFX("BuildCartas");
    }

    public void ClickAtras()
    {
        PlaySFX("ClickAtras");
    }

    public void ClickPausa()
    {
        PlaySFX("ClickPausa");
    }

    public void ClickSelect()
    {
        PlaySFX("ClickSelect");
    }

    public void Jugar()
    {
        PlaySFX("Jugar");
    }

    public void NuevoCliente()
    {
        PlaySFX("NuevoCliente");
    }

    public void MarcarCasilla()
    {
        PlaySFX("MarcarCasilla");
    }

    public void Desbloqueo()
    {
        PlaySFX("Desbloqueo");
    }
}