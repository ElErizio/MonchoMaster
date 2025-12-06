using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Layouts;
#endif


public class TutorialManager : MonoBehaviour
{
    [Header("Datos")]
    public List<TutorialStep> steps = new();
    public int startIndex = 0;

    [Header("UI")]
    public CanvasGroup panel;             // CanvasGroup para fade
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public Image imageHolder;

    [Header("FX")]
    public float fadeTime = 0.25f;

    int _index = -1;
    bool _running;
    float _savedTimeScale = 1f;

    void OnEnable() { TutorialEvents.OnEvent += OnTutorialEvent; }
    void OnDisable() { TutorialEvents.OnEvent -= OnTutorialEvent; }

    void Start()
    {
        // Oculta de inicio
        if (panel != null) { panel.alpha = 0; panel.gameObject.SetActive(false); }

        if (steps.Count > 0)
        {
            _index = Mathf.Clamp(startIndex, 0, steps.Count - 1);
            StartCoroutine(Run());
        }
    }

    IEnumerator Run()
    {
        _running = true;
        while (_running && _index < steps.Count)
        {
            var step = steps[_index];
            // UI fill
            if (titleText) titleText.text = step.title;
            if (bodyText) bodyText.text = step.body;
            if (imageHolder)
            {
                imageHolder.sprite = step.image;
                imageHolder.gameObject.SetActive(step.image != null);
            }

            // Fade in
            yield return Fade(true);

            // Pausa si aplica
            if (step.pauseGameplay)
            {
                _savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            // Espera de finalización
            yield return WaitForCompletion(step);

            // Unpause si aplica
            if (step.pauseGameplay)
            {
                Time.timeScale = _savedTimeScale;
            }

            // Fade out
            yield return Fade(false);

            _index++;
        }
        _running = false;
        // Al terminar puedes desactivar el GO del panel si quieres
    }

    IEnumerator WaitForCompletion(TutorialStep step)
    {
        switch (step.completeMode)
        {
            case TutorialStep.CompleteMode.AfterSeconds:
                // Si el juego está pausado, usa un contador independiente de TimeScale
                float t = 0f;
                while (t < step.seconds)
                {
                    t += step.pauseGameplay ? Time.unscaledDeltaTime : Time.deltaTime;
                    yield return null;
                }
                break;

            case TutorialStep.CompleteMode.OnInput:
                {
                    bool released = false;

#if ENABLE_INPUT_SYSTEM
                    // Usa New Input System: por defecto Space y A/B de gamepad
                    // Puedes ajustar el binding si quieres (ej.: "<Keyboard>/enter")
                    string binding = "<Keyboard>/space";
                    // Si quieres cambiar la tecla desde el Step.key, puedes mapear algunos casos comunes:
                    if (step.key == KeyCode.Return || step.key == KeyCode.KeypadEnter) binding = "<Keyboard>/enter";
                    else if (step.key == KeyCode.Escape) binding = "<Keyboard>/escape";
                    else if (step.key == KeyCode.Mouse0) binding = "<Mouse>/leftButton";
                    else if (step.key == KeyCode.Mouse1) binding = "<Mouse>/rightButton";
                    else if (step.key == KeyCode.Space) binding = "<Keyboard>/space";
                    // Agrega aquí más mapeos si los necesitas.

                    // Creamos una acción de botón en caliente
                    var action = new InputAction(type: InputActionType.Button);
                    action.AddBinding(binding);
                    // Extra: también aceptar botón sur de gamepad (A / Cross)
                    action.AddBinding("<Gamepad>/buttonSouth");

                    // Flag cuando se suelta el botón
                    action.performed += ctx => { released = true; };
                    action.Enable();

                    try
                    {
                        while (!released)
                            yield return null;
                    }
                    finally
                    {
                        action.Disable();
                        action.Dispose();
                    }
#else
    // Fallback: Old Input Manager (si algún día lo activas)
    while (!Input.GetKeyUp(step.key))
        yield return null;
#endif
                    break;
                }


            case TutorialStep.CompleteMode.OnEvent:
                _awaitingEvent = step.eventName;
                while (!string.IsNullOrEmpty(_awaitingEvent))
                    yield return null;
                break;
        }
    }

    string _awaitingEvent;

    void OnTutorialEvent(string evt)
    {
        if (!string.IsNullOrEmpty(_awaitingEvent) && evt == _awaitingEvent)
        {
            _awaitingEvent = null;
        }
    }

    IEnumerator Fade(bool show)
    {
        if (panel == null) yield break;

        panel.gameObject.SetActive(true);
        float from = panel.alpha;
        float to = show ? 1f : 0f;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            panel.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        panel.alpha = to;

        if (!show)
            panel.gameObject.SetActive(false);
    }
    public void SkipTo(int newIndex)
    {
        _index = Mathf.Clamp(newIndex, 0, steps.Count - 1);
    }
}
