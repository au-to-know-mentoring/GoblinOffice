using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{
    public SettingsData settingsData;
    // Start is called before the first frame update
    void Start()
    {
        settingsData = Resources.Load<SettingsData>("SettingsData");
        settingsData.difficultyMultiplier = 1.0f;
        settingsData.roomsCleared = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTheGame()
    {
        SceneManager.LoadScene("SampleScene 1");
    }

    public void LoadScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
}
