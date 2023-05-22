using UnityEngine;

[CreateAssetMenu(fileName = "SettingsData", menuName = "ScriptableObjects/SettingsData", order = 1)]
public class SettingsData : ScriptableObject
{
    // Add your settings variables here
    public bool musicEnabled;
    public int soundVolume;
    public int BeatsPerMinuteBPM;

    public Color Green1;
    public Color Red2;
    public Color Blue3;
    public Color Yellow4;
    
    // Add more settings as needed
}
