using UnityEngine;
using UnityEngine.UI;

public class LoteriaCellUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject markOverlay;

    [Header("Efectos de Marcado")]
    [SerializeField] private ParticleSystem markParticles;
    [SerializeField] private AudioSource markSound;

    public void Bind(Sprite sprite, bool marked, bool playEffects = false)
    {
        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = (sprite != null);
            icon.preserveAspect = true;
        }

        if (markOverlay != null)
        {
            markOverlay.SetActive(marked);
        }

        if (marked && playEffects && markParticles != null)
        {
            PlayMarkEffects();
        }
        else if (marked && !playEffects && markParticles != null)
        {
            markParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        else if (!marked && markParticles != null)
        {
            markParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void Bind(Sprite sprite, bool marked)
    {
        Bind(sprite, marked, false);
    }

    public void PlayMarkEffects()
    {
        // Activar partículas
        if (markParticles != null)
        {
            markParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            markParticles.Play();
        }

        /* Reproducir sonido
        if (markSound != null && markSound.clip != null)
        {
            markSound.Play();
        }*/

        if (markOverlay != null)
        {
            markOverlay.SetActive(true);
        }
    }
}