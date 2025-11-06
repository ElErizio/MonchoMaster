using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial/Step")]
public class TutorialStep : ScriptableObject
{
    public string id;                  // para analytics o debug
    [TextArea] public string title;
    [TextArea(3, 10)] public string body;
    public Sprite image;

    public enum CompleteMode { AfterSeconds, OnInput, OnEvent }
    public CompleteMode completeMode = CompleteMode.OnInput;

    [Header("AfterSeconds")]
    public float seconds = 2f;

    [Header("OnInput")]
    public KeyCode key = KeyCode.Space;   // si usas el New Input System, cámbialo a tu action

    [Header("OnEvent")]
    public string eventName;              // debe coincidir con TutorialEvents.Raise("...")

    [Header("Opciones")]
    public bool pauseGameplay = false;    // si quieres pausar TimeScale
}
