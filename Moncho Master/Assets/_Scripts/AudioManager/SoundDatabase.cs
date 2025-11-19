using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Audio/Sound Database")]
public class SoundDatabase : ScriptableObject
{
    public List<Sound> sounds = new List<Sound>();
}