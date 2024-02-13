using UnityEngine;
using UnityEditor;

public class SpriteSlicerEditor : EditorWindow
{
    private Texture2D textureToSlice;
    private int columns = 1;
    private int rows = 1;

    [MenuItem("Tools/Sprite Slicer")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSlicerEditor>("Sprite Slicer");
    }

    void OnGUI()
    {
        GUILayout.Label("Slice Texture2D into Sprites", EditorStyles.boldLabel);

        textureToSlice = (Texture2D)EditorGUILayout.ObjectField("Texture to Slice", textureToSlice, typeof(Texture2D), false);
        columns = EditorGUILayout.IntField("Columns", columns);
        rows = EditorGUILayout.IntField("Rows", rows);

        if (GUILayout.Button("Slice"))
        {
            SliceTexture();
        }
    }

    private void SliceTexture()
    {
        if (textureToSlice == null)
        {
            Debug.LogError("No texture selected.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(textureToSlice);
        string directory = System.IO.Path.GetDirectoryName(path);
        string exportedDirectory = System.IO.Path.Combine(directory, "Exported");
        if (!System.IO.Directory.Exists(exportedDirectory))
        {
            System.IO.Directory.CreateDirectory(exportedDirectory);
        }

        int spriteWidth = textureToSlice.width / columns;
        int spriteHeight = textureToSlice.height / rows;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Rect spriteRect = new Rect(x * spriteWidth, (rows - y - 1) * spriteHeight, spriteWidth, spriteHeight);
                Sprite newSprite = Sprite.Create(textureToSlice, spriteRect, new Vector2(0.5f, 0.5f));
                string spritePath = System.IO.Path.Combine(exportedDirectory, textureToSlice.name + "_" + x + "_" + y + ".asset");
                AssetDatabase.CreateAsset(newSprite, spritePath);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Sprites saved to " + exportedDirectory);
    }
}