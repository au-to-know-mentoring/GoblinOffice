using UnityEngine;

public class SpriteAnimationCopier : MonoBehaviour
{
    public Sprite[] sprites;

    void Start()
    {
        // Access individual sprites by index
        int spriteIndex = 0; // The index of the sprite you want to access
        if (spriteIndex >= 0 && spriteIndex < sprites.Length)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprites[spriteIndex];
        }
        else
        {
            Debug.LogError("Invalid sprite index!");
        }
    }
}