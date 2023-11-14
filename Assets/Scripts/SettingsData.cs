using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

[CreateAssetMenu(fileName = "SettingsData", menuName = "ScriptableObjects/SettingsData", order = 1)]
public class SettingsData : ScriptableObject
{
    // Add your settings variables here
    public bool musicEnabled;
    public bool debugMode;
    public int soundVolume;
    public int BeatsPerMinuteBPM;
    public float BeatsPerSecondBPM;
    public float GlobalSettingsTimer;

    public Color Green1;
    public Color Red2;
    public Color Blue3;
    public Color Yellow4;

    // Add more settings as needed




    
}
