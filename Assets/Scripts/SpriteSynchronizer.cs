using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteSynchronizer : MonoBehaviour
{
    public SpriteRenderer sourceSpriteRenderer;
    public SpriteRenderer CopySpriteRenderer;
    public SettingsData GlobalSettingsObject;
    public PathfindingObject myPathFindingObject;

    Color myColour = Color.black;
    // public GameObject myObstacleGameObject; // This will be changed.
    [SerializeField]
    [Tooltip("Can be set manually, otherwise It will look for the Sprite sheets name + Atlas")]
    private SpriteAtlas spriteAtlas;
    [Header("View Only")]
    [SerializeField]
    private string SourceTitle;
    private string originalSourceTitle;
    [SerializeField]
    private string TargetTitle;
    [SerializeField]
    private string atlasBaseName;
    private void Start()
    {
        if (GlobalSettingsObject == null)
        {
            GlobalSettingsObject = Resources.Load<SettingsData>("SettingsData");
            Debug.Log($"[SpriteSynchronizer] Loaded GlobalSettingsObject: {GlobalSettingsObject != null}");
        }
        if (sourceSpriteRenderer == null || CopySpriteRenderer == null)
        {
            Debug.LogError("[SpriteSynchronizer] Missing renderer references on " + gameObject.name);
            return;
        }
        if (sourceSpriteRenderer.gameObject.GetComponent<PathfindingObject>() != null)
        {
            myPathFindingObject = sourceSpriteRenderer.gameObject.GetComponent<PathfindingObject>();
            Debug.Log($"[SpriteSynchronizer] Found PathfindingObject with color: {myPathFindingObject.myColour}");
        }
        originalSourceTitle = sourceSpriteRenderer.sprite.name;
        SourceTitle = Regex.Replace(sourceSpriteRenderer.sprite.name, "[0-9_]", "");
        TargetTitle = Regex.Replace(CopySpriteRenderer.sprite.name, "[0-9_]", "");


        if (spriteAtlas == null)
        {
            spriteAtlas = Resources.Load<SpriteAtlas>(SourceTitle + "Atlas"); // (NameOfSprite + Atlas.) = (TrollNinjaAtlas)
        }

        if (spriteAtlas == null)
        {
            Debug.LogError("Source sprite Atlas is not found on " + gameObject.name);
            return;
        }

        if (spriteAtlas != null)
        {
            InitializeAtlasBaseName();
        }

        // Synchronize the sprite initially
        SyncSprites();
    }
    private string ExtractStringNumberFromSpriteName(string spriteName)
    {
        return Regex.Match(spriteName, @"\d+$").Value;
    }
    private void InitializeAtlasBaseName()
    {
        Sprite[] allSprites = new Sprite[spriteAtlas.spriteCount];
        spriteAtlas.GetSprites(allSprites);

        if (allSprites.Length > 0)
        {
            string firstSpriteName = allSprites[0].name;
            atlasBaseName = Regex.Replace(firstSpriteName, @"_\d+\(Clone\)$", "");
            Debug.Log($"Atlas base name: {atlasBaseName}");
        }
        else
        {
            Debug.LogError("No sprites found in the atlas.");
        }
    }

    
    private void Update()
    {
        // Continuously check and synchronize sprites
        SyncSprites();
    }

    private void SyncSprites()
    {
        if (sourceSpriteRenderer.sprite == null)
        {
            Debug.Log("SourceSpriteRenderer is null.");
            return;
        }

        CopySpriteRenderer.flipX = sourceSpriteRenderer.flipX;

        // Get the numerical value following the underscore from the source sprite name
        //int spriteNumber = ExtractNumberFromSpriteName(sourceSpriteRenderer.sprite.name);
        string stringSpriteNumber = ExtractStringNumberFromSpriteName(sourceSpriteRenderer.sprite.name);
        // Set the sprite on the target GameObject using the numerical value
        //SetSpriteByNumber(spriteNumber);


        SetSpriteByStringNumber(stringSpriteNumber);
        int myColour = 0;
        if (myPathFindingObject != null)
        {
            myColour = (int)myPathFindingObject.myColour;
            Debug.Log($"[SpriteSynchronizer] Updating sprite color. PathfindingObject color: {myPathFindingObject.myColour}, Cast to int: {myColour}, GlobalSettings null?: {GlobalSettingsObject == null}");
        }

        switch (myColour)
        {
            case 0:
                CopySpriteRenderer.color = Color.white;
                Debug.Log("White Color Code");
                break;
            case 1:
                CopySpriteRenderer.color = GlobalSettingsObject.Green1;
                Debug.Log("Green Color Code");
                break;
            case 2:
                CopySpriteRenderer.color = GlobalSettingsObject.Red2;
                Debug.Log("Red Color Code");
                break;
            case 3:
                CopySpriteRenderer.color = GlobalSettingsObject.Blue3;
                Debug.Log("Blue Color Code");
                break;
            case 4:
                CopySpriteRenderer.color = GlobalSettingsObject.Yellow4;
                Debug.Log("Yellow Color Code");
                break;
            default:
                Debug.Log("Unknown Color Code");
                break;
        }
    }

    //private int ExtractNumberFromSpriteName(string spriteName)
    //{
    //    int number = -1;
    //    string result = Regex.Replace(spriteName, "[^0-9]", "");
    //    if (int.TryParse(result, out number))
    //    {
    //        return number;
    //    }

    //    //ALl below unneccessary?
    //    int underscoreIndex = spriteName.LastIndexOf('_');
    //    if (underscoreIndex >= 0 && underscoreIndex < spriteName.Length - 1)
    //    {
    //        string numberString = spriteName.Substring(underscoreIndex + 1);
    //        if (int.TryParse(numberString, out number))
    //        {
    //            return number;
    //        }
    //    }
    //    return number;
    //}


    private void SetSpriteByStringNumber(string number)
    {
        if (spriteAtlas == null || string.IsNullOrEmpty(atlasBaseName))
        {
            return;
        }

        string targetSpriteName = atlasBaseName + "_" + number;
        Sprite targetSprite = spriteAtlas.GetSprite(targetSpriteName);

        if (targetSprite == null)
        {
            // Try without underscore
            targetSpriteName = atlasBaseName + number;
            targetSprite = spriteAtlas.GetSprite(targetSpriteName);
        }

        if (targetSprite == null)
        {
            Debug.LogError($"Sprite not found: {targetSpriteName}. SpriteCount of atlas is: {spriteAtlas.spriteCount}");
        }
        else
        {
            CopySpriteRenderer.sprite = targetSprite;
        }
    }

}
