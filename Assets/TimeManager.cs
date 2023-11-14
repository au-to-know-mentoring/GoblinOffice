using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{

    public float Timer;
    public SettingsData GlobalSettings;



    // Start is called before the first frame update
    void Start()
    {
        Timer = 0f;
        GlobalSettings.BeatsPerSecondBPM = (float)GlobalSettings.BeatsPerMinuteBPM / 60;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
