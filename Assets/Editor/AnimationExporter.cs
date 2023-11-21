using UnityEngine;
using UnityEditor;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AnimationExporter : EditorWindow
{
    [MenuItem("Tools/Export Animation to XML")]
    static void Init()
    {
        AnimationExporter window = (AnimationExporter)EditorWindow.GetWindow(typeof(AnimationExporter));
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Export Animation"))
        {
            ExportAnimation();
        }
    }

    void ExportAnimation()
    {
        // Get the currently selected game object
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogWarning("No game object selected. Please select a game object with an animation.");
            return;
        }

        // Assuming there's only one animator component
        Animator animator = selectedObject.GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning("Selected game object does not have an Animator component.");
            return;
        }

        // Create a class to hold frame data
        AnimationData animationData = new AnimationData();

        // Get the sprite renderer component
        SpriteRenderer spriteRenderer = selectedObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning("Selected game object does not have a SpriteRenderer component.");
            return;
        }

        // Get the number of frames in the animation
        AnimationClip myClip = animator.runtimeAnimatorController.animationClips[0];
        float frames = myClip.frameRate * myClip.length;
        int totalFrames = Mathf.RoundToInt(frames);
        int totalAnimations = animator.runtimeAnimatorController.animationClips.Length;
        for (int animationIndex = 0; animationIndex < totalAnimations; animationIndex++)
        {
            //[TODO] frame index needs to keep going up it cannot reset or the sprite names will be wrong.
            //[TODO] The structure of this loop needs to be changed. Possibly add the length of all previous animations each time?
            for (int frameIndex = 0; frameIndex < totalFrames; frameIndex++)
            {
                // Set the animator to the current frame
                float normalizedTime = (float)frameIndex / totalFrames;
                animator.Play(0, 0, normalizedTime);

                // Attempt to get sprite name from animation clip
                string spriteName = GetSpriteNameFromAnimationClip(animator, frameIndex, spriteRenderer);

                // Populate frame data
                FrameData frameData = new FrameData();
                //[TODO] Get animation clip name
                //frameData.AnimationName =
                frameData.Size = spriteRenderer.sprite.bounds.size;
                frameData.SpriteName = spriteName ?? "UnknownSprite";
                frameData.FrameIndex = frameIndex;
                // Add more properties as needed

                // Add the frame data to the animation data
                animationData.Frames.Add(frameData);
            }
        }
        // Serialize animation data to XML
        XmlSerializer serializer = new XmlSerializer(typeof(AnimationData));
        using (FileStream stream = new FileStream("Assets/animation_data.xml", FileMode.Create))
        {
            serializer.Serialize(stream, animationData);
        }

        Debug.Log("Animation exported to XML: Assets/animation_data.xml");
    }

    // Helper method to get sprite name from animation clip
    string GetSpriteNameFromAnimationClip(Animator animator, int frameIndex, SpriteRenderer spriteRenderer)
    {
        // Get the sprite name from the spriteRenderer's sprite

        string baseSpriteName = spriteRenderer.sprite.name;
        string EditedSpriteName = Regex.Replace(baseSpriteName, "[0-9_]", "");
        // Append "_x" to the sprite name, where x is the frame index
        return $"{EditedSpriteName}_{frameIndex}";
    }
}

[System.Serializable]
public class AnimationData
{
    public List<FrameData> Frames = new List<FrameData>();
}

[System.Serializable]
public class FrameData
{
    public string AnimationName;
    public Vector2 Size;
    public string SpriteName;
    public int FrameIndex;
    // Add more properties as needed
}
