using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class BeatEventManager : MonoBehaviour
{
    public int bpm = 120; // Beats per minute
    public List<BeatEvent> beatEvents = new List<BeatEvent>();
    public SettingsData GlobalSettingsObject;
    private int currentBeat = 0; // Current beat count
    private float beatInterval; // Interval between beats in seconds
    private float nextBeatTime; // Time of the next beat
    private List<BeatEvent> eventsToRemove = new List<BeatEvent>(); // List to store events to be removed
    private void Start()
    {
        // Calculate the beat interval based on the BPM
        if(GlobalSettingsObject!= null)
        bpm = GlobalSettingsObject.BeatsPerMinuteBPM;
        beatInterval = 60f / bpm;
        nextBeatTime = Time.time + beatInterval;
    }

    private void Update()
           {
        // Check if it's time for the next beat
        if (Time.time >= nextBeatTime)
        {
            // Trigger the beat event
            TriggerBeatEvent();

            // Update the time of the next beat
            nextBeatTime += beatInterval;
            currentBeat++;
        }
    }

    private void TriggerBeatEvent()
    {
        eventsToRemove.Clear(); // Clear the list of events to be removed

        // Trigger each beat event in the list on the corresponding beat
        foreach (var beatEvent in beatEvents)
        {
            if (currentBeat == 0) // Causes all beats to play instantly if you don't return on 0
                return;

            if (currentBeat % beatEvent.beatNumber == 0)
            {
                beatEvent.OnBeat();
                if (!beatEvent.repeat)
                {
                    beatEvent.flagToRemove = true;
                    if (beatEvent.flagToRemove)
                    {
                        eventsToRemove.Add(beatEvent); // Add the event to the removal list
                    }
                }
            }
        }

        // Remove the flagged events after the loop
        foreach (var eventToRemove in eventsToRemove)
        {
            beatEvents.Remove(eventToRemove);
        }
    }

    [System.Serializable]
    public class BeatEvent
    {
        public string eventName;
        public int beatNumber;
        public bool repeat;
        public bool flagToRemove;
        public void OnBeat()
        {
            Debug.Log(eventName + " triggered on beat " + beatNumber);
            // Add your event logic here
        }
    }
}