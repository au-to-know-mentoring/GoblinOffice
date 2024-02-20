using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.U2D;
using UnityEditor.U2D;

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

        MakeTextureReadable(path);
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
        SpriteAtlas spriteAtlas = new SpriteAtlas();
        SpriteAtlasUtility.PackTextures(spriteAtlas, textureToSlice, exportedDirectory, columns, rows);

        AssetDatabase.SaveAssets();
        Debug.Log("Sprites and SpriteAtlas saved to " + exportedDirectory);
    }

    public static class SpriteAtlasUtility
    {
        public static void PackTextures(SpriteAtlas spriteAtlas, Texture2D textureToSlice, string exportedDirectory, int columns, int rows)
        {
            int spriteWidth = textureToSlice.width / columns;
            int spriteHeight = textureToSlice.height / rows;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    Rect spriteRect = new Rect(x * spriteWidth, textureToSlice.height - (y + 1) * spriteHeight, spriteWidth, spriteHeight);
                    Texture2D newTexture = new Texture2D((int)spriteRect.width, (int)spriteRect.height);
                    newTexture.SetPixels(textureToSlice.GetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height));
                    newTexture.Apply();

                    byte[] bytes = newTexture.EncodeToPNG();
                    //string texturePath = Path.Combine(exportedDirectory, textureToSlice.name + "_" + x + "_" + y + ".png");
                    string texturePath = Path.Combine(exportedDirectory, textureToSlice.name + "_" + (x + (10*y)) + ".png");
                    File.WriteAllBytes(texturePath, bytes);
                    AssetDatabase.ImportAsset(texturePath);

                    TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();

                    Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                    spriteAtlas.Add(new[] { newSprite });
                }
            }

            string atlasPath = Path.Combine(exportedDirectory, textureToSlice.name + "_SpriteAtlas.spriteatlas");
            AssetDatabase.CreateAsset(spriteAtlas, atlasPath);
        }
    }

    private static void MakeTextureReadable(string texturePath)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (textureImporter != null && !textureImporter.isReadable)
        {
            textureImporter.isReadable = true;
            textureImporter.SaveAndReimport();
        }
    }
}