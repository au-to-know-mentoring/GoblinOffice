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
    public int TrueBeats = 0;

    [Range(1, 5)]
    public float difficultyMultiplier = 1.1f; // Adjustable in the Inspector

    public int totalBeats = 10; // Total number of beats in a loop

    // Function to roll for BeatEvents on each beat based on difficulty
    public bool[] RollBeatEvents()
    {
        bool[] beatEvents = new bool[totalBeats];
        for (int i = 0; i < totalBeats; i++)
        {
            // For the first beat, there is no previous beat, so pass false as the default value
            bool previousBeatHadEvent = i > 0 && beatEvents[i - 1];
            beatEvents[i] = RollForBeatEvent(previousBeatHadEvent);
        }
        return beatEvents;
    }

    private bool RollForBeatEvent(bool previousBeatHadEvent)
    {
        // Base probability of a BeatEvent occurring on any given beat
        float baseProbability = 0.2f; // 20% chance

        // Adjust probability based on difficulty
        float difficultyAdjustment = (difficultyMultiplier - 1) / 4.0f;
        float adjustedProbability = baseProbability + (difficultyAdjustment * (1 - baseProbability));

        // If the previous beat had an event, reduce the probability for this beat
        if (previousBeatHadEvent)
        {
            adjustedProbability *= difficultyMultiplier / 5f; // Example: halve the probability
        }

        // Roll for BeatEvent
        return Random.value < adjustedProbability;
    }
}
