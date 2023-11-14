using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIImageSpawner : MonoBehaviour
{
    public GameObject imagePrefab;
    public RectTransform spawnPoint;
    public float spawnInterval = 1.0f;
    public float ScrollSpeed = 1.0f;
    int beat = 1;
    int beatLoop;
    private float timer;

    Vector3 spawnHeight;
    public TimeManager timeManager;
    private void Start()
    {
        ScrollSpeed = (float)Screen.height * timeManager.GlobalSettings.BeatsPerSecondBPM / 2;
        timer = spawnInterval;
        spawnHeight = new Vector3(0, GetComponent<RectTransform>().rect.height / 2, 0);
    }
    private void Update()
    {
        timer += Time.deltaTime * timeManager.GlobalSettings.BeatsPerSecondBPM;

        if (timer >= spawnInterval)
        {
            SpawnImage();
            beat++;
            if(beat > beatLoop)
            {
                beat = 1;
            }
            timer = timer - spawnInterval;
        }
    }

    private void SpawnImage()
    {
        
        GameObject newImageObj = Instantiate(imagePrefab, Vector3.zero, Quaternion.identity);
        newImageObj.transform.SetParent(spawnPoint, false);

        RectTransform rectTransform = newImageObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2((Screen.width / 2), Screen.height / 2); // Spawn at top right corner.

        UIImageScroll script = newImageObj.GetComponent<UIImageScroll>();
        script.scrollSpeed = ScrollSpeed;
        script.despawnThreshold = 0; // Halfway down the screen.

        Text newImageText = newImageObj.GetComponentInChildren<Text>();
        newImageText.text = beat.ToString();

    }

    public void setBeatLoop(int BeatToLoop)
    {
        beatLoop= BeatToLoop;
    }
}
