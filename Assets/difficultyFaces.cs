using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class difficultyFaces : MonoBehaviour
{
    public Sprite[] difficultyFacesArray;
    public Image face;
    public SettingsData GlobalSettingsObject;
    void Start()
    {
        int faceToUse = (int)GlobalSettingsObject.difficultyMultiplier;
        face.sprite = difficultyFacesArray[faceToUse - 1];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
