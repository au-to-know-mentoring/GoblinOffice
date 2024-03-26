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

    int myColour = 0;
    public PathfindingObject myPathFindingObject;
    [SerializeField]
    [Tooltip("Can be set manually, otherwise It will look for the Sprite sheets name + Atlas")]
    private SpriteAtlas spriteAtlas;
    [Header("View Only")]
    [SerializeField]
    private string SourceTitle;
    private string originalSourceTitle;
    [SerializeField]
    private string TargetTitle;

    private void Start()
    {
        GlobalSettingsObject = Resources.Load<SettingsData>("SettingsData");
        if (sourceSpriteRenderer == null || CopySpriteRenderer == null)
        {
            Debug.LogError("Source GameObject or Target SpriteRenderer is not set in the SpriteSynchronizer script on " + gameObject.name);
            return;
        }
        if (sourceSpriteRenderer.gameObject.GetComponent<PathfindingObject>() != null)
        {
            myPathFindingObject = sourceSpriteRenderer.gameObject.GetComponent<PathfindingObject>();
        }
            originalSourceTitle = sourceSpriteRenderer.name;
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


        // Synchronize the sprite initially
        SyncSprites();
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
            return;
        }

        // Get the numerical value following the underscore from the source sprite name
        //int spriteNumber = ExtractNumberFromSpriteName(sourceSpriteRenderer.sprite.name);
        string stringSpriteNumber = ExtractStringNumberFromSpriteName(sourceSpriteRenderer.sprite.name);
        // Set the sprite on the target GameObject using the numerical value
        //SetSpriteByNumber(spriteNumber);


        SetSpriteByStringNumber(stringSpriteNumber);
        
        if (myPathFindingObject != null)
            myColour = (int)myPathFindingObject.myColour;

        switch (myColour)
        {
            case 0:
                
                CopySpriteRenderer.color = Color.white;
                break;
            case 1:
                
                CopySpriteRenderer.color = GlobalSettingsObject.Green1;
                break;
            case 2:
                
                CopySpriteRenderer.color = GlobalSettingsObject.Red2;
                break;
            case 3:
                
                CopySpriteRenderer.color = GlobalSettingsObject.Blue3;
                break;
            case 4:
                
                CopySpriteRenderer.color = GlobalSettingsObject.Yellow4;
                break;
            default:
                Console.WriteLine("Unknown Color Code");
                break;
        }
    }

    private int ExtractNumberFromSpriteName(string spriteName)
    {
        int number = -1;
        string result = Regex.Replace(spriteName, "[^0-9]", "");
        if (int.TryParse(result, out number))
        {
            return number;
        }

        //ALl below unneccessary?
        int underscoreIndex = spriteName.LastIndexOf('_');
        if (underscoreIndex >= 0 && underscoreIndex < spriteName.Length - 1)
        {
            string numberString = spriteName.Substring(underscoreIndex + 1);
            if (int.TryParse(numberString, out number))
            {
                return number;
            }
        }
        return number;
    }

    private string ExtractStringNumberFromSpriteName(string spriteName)
    {
        string result = Regex.Replace(spriteName, "[^0-9]", "");
        return result;
    }

    private void SetSpriteByNumber(int number)
    {
        if (number < 0 || spriteAtlas == null)
        {
            return;
        }

        // Assuming the sprite sheet has been sliced evenly
        string targetSpriteName;
        if(SourceTitle.Contains("_"))
        {
            targetSpriteName = TargetTitle + "_" + number.ToString();
        }
        else
        {
            targetSpriteName = TargetTitle; // targetSpriteName = TargetTitle + "PARTCLE" + number.ToString();  // Change this.
        }

        Sprite targetSprite = spriteAtlas.GetSprite(targetSpriteName);  

        if (targetSprite != null)
        {
            CopySpriteRenderer.sprite = targetSprite;
        }
    }

    private void SetSpriteByStringNumber(string number)
    {
        if (spriteAtlas == null)
        {
            return;
        }

        // Assuming the sprite sheet has been sliced evenly
        string targetSpriteName;
        if (originalSourceTitle.Contains("_"))
        {
            targetSpriteName = TargetTitle + "_" + number;
        }
        else
        {
            targetSpriteName = TargetTitle + number; // targetSpriteName = TargetTitle + "PARTCLE" + number.ToString();  // Change this.
        }

        Sprite targetSprite = spriteAtlas.GetSprite(targetSpriteName);

        if (targetSprite != null)
        {
            CopySpriteRenderer.sprite = targetSprite;
        }
    }
}
