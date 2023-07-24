using UnityEngine;

public class SpriteSheetController : MonoBehaviour
{
    public Texture2D spriteSheet;
    public int rows = 4; // Adjust these values based on your sprite sheet layout
    public int columns = 4;
    public float padding = 0.1f; // Adjust this value to add padding between sprites
    public Sprite[] spriteArray;

    public SpriteRenderer spriteNameHolder; // Reference to the object containing the sprite name

    void Start()
    {
        // Calculate the width and height of each individual sprite in the sprite sheet
        int spriteWidth = spriteSheet.width / columns;
        int spriteHeight = spriteSheet.height / rows;

        // Calculate the size of the padding between sprites
        int paddingWidth = (int)(spriteWidth * padding);
        int paddingHeight = (int)(spriteHeight * padding);

        // Initialize the sprite array
        spriteArray = new Sprite[rows * columns];

        // Loop through the rows and columns of the sprite sheet
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Calculate the position and size of the current sprite
                int x = col * (spriteWidth + paddingWidth);
                int y = row * (spriteHeight + paddingHeight);
                Rect spriteRect = new Rect(x, y, spriteWidth, spriteHeight);

                // Extract the sprite from the sprite sheet texture
                Sprite sprite = Sprite.Create(spriteSheet, spriteRect, new Vector2(0.5f, 0.5f), 100);

                // Add the sprite to the sprite array
                int index = row * columns + col;
                spriteArray[index] = sprite;
            }
        }


    }

    private void Update()
    {
        // Get the sprite name from the spriteNameHolder object and extract the number from it
        int spriteIndex = ExtractNumberFromSpriteName(spriteNameHolder.sprite.name);

        // Set the sprite with the extracted index to the SpriteRenderer
        if (spriteIndex >= 0 && spriteIndex < spriteArray.Length)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = spriteArray[spriteIndex];
        }
    }

    // Helper method to extract numbers from a given string
    int ExtractNumberFromSpriteName(string name)
    {
        string numberString = "";
        foreach (char c in name)
        {
            if (char.IsDigit(c))
            {
                numberString += c;
            }
        }
        int number;
        int.TryParse(numberString, out number);
        return number;
    }
}