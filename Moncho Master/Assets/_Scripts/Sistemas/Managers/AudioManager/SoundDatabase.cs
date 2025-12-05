using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Audio/Sound Database")]
public class SoundDatabase : ScriptableObject
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    public List<Sound> sounds = new List<Sound>();

    public AudioClip GetAudioClip(string soundName)
    {
        foreach (Sound sound in sounds)
        {
            if (sound.name == soundName)
            {
                return sound.clip;
            }
        }

        Debug.LogWarning($"AudioClip '{soundName}' no encontrado en SoundDatabase");
        return null;
    }
}