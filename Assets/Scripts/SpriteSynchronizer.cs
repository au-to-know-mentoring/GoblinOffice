using System.ComponentModel;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteSynchronizer : MonoBehaviour
{
    public SpriteRenderer sourceSpriteRenderer;
    public SpriteRenderer targetSpriteRenderer;

    [SerializeField]
    [Tooltip("Can be set manually, otherwise It will look for the Sprite sheets name + Atlas")]
    private SpriteAtlas spriteAtlas;

    private void Start()
    {
        if (sourceSpriteRenderer == null || targetSpriteRenderer == null)
        {
            Debug.LogError("Source GameObject or Target SpriteRenderer is not set in the SpriteSynchronizer script on " + gameObject.name);
            return;
        }

        if (spriteAtlas == null)
        {
            string SourceTitle = Regex.Replace(sourceSpriteRenderer.sprite.name, "[0-9_]", "") + "Atlas";
            spriteAtlas = Resources.Load<SpriteAtlas>(SourceTitle); // (NameOfSprite + Atlas.) = (TrollNinjaAtlas)
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
        int spriteNumber = ExtractNumberFromSpriteName(sourceSpriteRenderer.sprite.name);

        // Set the sprite on the target GameObject using the numerical value
        SetSpriteByNumber(spriteNumber);
    }

    private int ExtractNumberFromSpriteName(string spriteName)
    {
        int number = -1;
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

    private void SetSpriteByNumber(int number)
    {
        if (number < 0 || spriteAtlas == null)
        {
            return;
        }

        // Assuming the sprite sheet has been sliced evenly
        string targetSpriteName = "gunparticleSprite_" + number.ToString();
        Sprite targetSprite = spriteAtlas.GetSprite(targetSpriteName);

        if (targetSprite != null)
        {
            targetSpriteRenderer.sprite = targetSprite;
        }
    }
}
