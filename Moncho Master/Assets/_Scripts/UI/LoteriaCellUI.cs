using UnityEngine;
using UnityEngine.UI;

public class LoteriaCellUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject markOverlay;

    [Header("Efectos de Marcado")]
    [SerializeField] private ParticleSystem markParticles;

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
        AudioManager.Instance.MarcarCasilla();

        if (markParticles != null)
        {
            markParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            markParticles.Play();
        }

        if (markOverlay != null)
        {
            markOverlay.SetActive(true);
        }
    }
}