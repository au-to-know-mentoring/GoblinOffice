using UnityEngine;
using UnityEditor;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AnimationExporter : EditorWindow
{
    // Declare instance variables
    private GameObject selectedObject;
    private Animator animator;
    private AnimationData animationData;
    private SpriteRenderer spriteRenderer;
    private AnimationClip myClip;
    private int frameIndex;

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
            EditorApplication.update += ExportAnimation;
        }
    }

    void ExportAnimation()
    {
        // Initialize instance variables
        selectedObject = Selection.activeGameObject;
        animator = selectedObject.GetComponent<Animator>();
        animationData = new AnimationData();
        spriteRenderer = selectedObject.GetComponent<SpriteRenderer>();


        // Get the number of animations
        int totalAnimations = animator.runtimeAnimatorController.animationClips.Length;

        // Iterate over all animations
        for (int animationIndex = 0; animationIndex < totalAnimations; animationIndex++)
        {
            myClip = animator.runtimeAnimatorController.animationClips[animationIndex];
            float frames = myClip.frameRate * myClip.length;
            int totalFrames = Mathf.RoundToInt(frames);

            for (frameIndex = 0; frameIndex < totalFrames; frameIndex++)
            {
                // Set the animator to the current frame
                float normalizedTime = (float)frameIndex / totalFrames;
                animator.Play(myClip.name, 0, normalizedTime);

                // Wait for a small delay to give the Animator time to update
                EditorApplication.update -= ExportAnimation;
                EditorApplication.update += WaitForAnimationUpdate;

                return;
            }
        }

 
    }

    void WaitForAnimationUpdate()
    {
        // Get the sprite name from the SpriteRenderer
        string spriteName = spriteRenderer.sprite.name;

        // Populate frame data
        FrameData frameData = new FrameData();
        frameData.AnimationName = myClip.name;
        frameData.SpriteName = spriteName ?? "UnknownSprite";
        frameData.FrameIndex = frameIndex;

        // Add the frame data to the animation data
        animationData.Frames.Add(frameData);

        EditorApplication.update -= WaitForAnimationUpdate;
        EditorApplication.update += ExportAnimation;
    }

    [System.Serializable]
    public class AnimationData
    {
        public List<FrameData> Frames = new List<FrameData>();
    }

    [System.Serializable]
    public class FrameData
    {
        public string AnimationName; // Generate based of enemy name, Declare start and end frame
                                     //public Vector2 Size;
        public string SpriteName;
        public int FrameIndex;
        // Add more properties as needed
    }
}
