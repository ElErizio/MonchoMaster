using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial/Step")]
public class TutorialStep : ScriptableObject
{
    public string id;
    [TextArea] public string title;
    [TextArea(3, 10)] public string body;
    public Sprite image;

    public enum CompleteMode { AfterSeconds, OnInput, OnEvent }
    public CompleteMode completeMode = CompleteMode.OnInput;

    [Header("AfterSeconds")]
    public float seconds = 2f;

    [Header("OnInput")]
    public KeyCode key = KeyCode.Space;

    [Header("OnEvent")]
    public string eventName;

    [Header("Opciones")]
    public bool pauseGameplay = false;
}
