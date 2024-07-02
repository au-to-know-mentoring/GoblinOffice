using UnityEngine;

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
    public int roomsCleared = 0;

    public Color Green1;
    public Color Red2;
    public Color Blue3;
    public Color Yellow4;

    // Add more settings as needed
    public int TrueBeats = 0;

    [Range(1, 5)]
    public float difficultyMultiplier = 1.0f; // Adjustable in the Inspector

    public int totalBeats = 10; // Total number of beats in a loop

    // Function to roll for BeatEvents on each beat based on difficulty
    public bool[] RollBeatEvents()
    {
        bool[] beatEvents = new bool[totalBeats];
        beatEvents[0] = false;
        beatEvents[1] = false;
        beatEvents[2] = false;
        for (int i = 2; i < totalBeats - 2; i++)
        {
            // For the first beat, there is no previous beat, so pass false as the default value
            bool previousBeatHadEvent = i > 0 && beatEvents[i - 1];
            beatEvents[i] = RollForBeatEvent(previousBeatHadEvent);
        }

        // Check if all beats are false
        bool allFalse = true;
        for (int i = 2; i < totalBeats - 2; i++)
        {
            if (beatEvents[i])
            {
                allFalse = false;
                break;
            }
        }

        // If all beats are false, randomly set one to true
        if (allFalse)
        {
            int randomIndex = UnityEngine.Random.Range(3, totalBeats - 2);
            beatEvents[randomIndex] = true;
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
            if(difficultyMultiplier == 1.0f)
            {
                adjustedProbability = 0f;
            }
            adjustedProbability *= difficultyMultiplier / 5f; // Example: halve the probability
        }

        // Roll for BeatEvent
        return Random.value < adjustedProbability;
    }
}
