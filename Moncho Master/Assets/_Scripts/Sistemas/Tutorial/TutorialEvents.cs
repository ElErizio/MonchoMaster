using System;
public static class TutorialEvents
{
    public static event Action<string> OnEvent;

    public static void Raise(string eventName)
    {
        OnEvent?.Invoke(eventName);
    }
}
