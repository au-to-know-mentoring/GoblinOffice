using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;

public class PrefabDuplicator : MonoBehaviour
{
    public GameObject prefab; // The prefab to copy
    public Sprite[] oldSpriteSheet; // The original sprite sheet
    public Sprite[] newSpriteSheet; // The new sprite sheet
    public string newPrefabPath = "Assets/NewPrefab.prefab"; // The path to save the new prefab
    public Animator animator;
    GameObject newObject;
    void Start()
    {
        // Instantiate a copy of the prefab
        newObject = Instantiate(prefab);
        newObject.transform.position = new Vector3(2, 2, 0);

        // Get the Animator component
        animator = newObject.GetComponent<Animator>();

        // Check if the Animator component exists
        //if (animator != null)
        //{
        //    // Get the RuntimeAnimatorController from the Animator
        //    RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;

        //    // Check if the RuntimeAnimatorController exists
        //    if (runtimeAnimatorController != null)
        //    {
        //        // Get the path of the original AnimatorController
        //        string originalPath = AssetDatabase.GetAssetPath(runtimeAnimatorController);

        //        // Create a new path for the duplicate AnimatorController
        //        string newPath = System.IO.Path.GetDirectoryName(newPrefabPath) + "/" + runtimeAnimatorController.name + "_copy.controller";

        //        // Create a copy of the AnimatorController
        //        AssetDatabase.CopyAsset(originalPath, newPath);

        //        // Assign the new AnimatorController to the Animator
        //        AnimatorController newAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(newPath);
        //        animator.runtimeAnimatorController = newAnimatorController;

        //        // Iterate over the layers
        //        for (int i = 0; i < newAnimatorController.layers.Length; i++)
        //        {
        //            var layer = newAnimatorController.layers[i];
        //            var stateMachine = layer.stateMachine;

        //            // Iterate over the states
        //            for (int j = 0; j < stateMachine.states.Length; j++)
        //            {
        //                var state = stateMachine.states[j].state;

        //                // Check if the state's motion is an AnimationClip
        //                if (state.motion is AnimationClip oldClip)
        //                {
        //                    // Get the path of the original AnimationClip
        //                    string originalClipPath = AssetDatabase.GetAssetPath(oldClip);

        //                    // Create a new path for the duplicate AnimationClip
        //                    string newClipPath = System.IO.Path.GetDirectoryName(newPrefabPath) + "/" + oldClip.name + "_copy.anim";

        //                    // Create a copy of the AnimationClip
        //                    AssetDatabase.CopyAsset(originalClipPath, newClipPath);

        //                    // Load the new AnimationClip
        //                    AnimationClip newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(newClipPath);

        //                    // Replace the old AnimationClip with the new one in the state
        //                    state.motion = newClip;
        //                }
        //            }
        //        }
        //    }
        //}
        //CreateAnimationClip(newSpriteSheet, 10, 18, 12f, "152");
        //Save the new object as a prefab
        //PrefabUtility.SaveAsPrefabAsset(newObject, newPrefabPath);

        // Destroy the new object from the scene
    }

    public AnimationClip CreateAnimationClip(Sprite[] sprites, int startFrame, int endFrame , float frameRate, string name)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = frameRate;  // frames per second
        
        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";


        ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Length];
        endFrame++;
        for (int i = 0; i < (endFrame - startFrame); i++)
        {
            spriteKeyFrames[i] = new ObjectReferenceKeyframe();
            spriteKeyFrames[i].time = i / frameRate;
            spriteKeyFrames[i].value = sprites[i + startFrame];
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);


        // Save the AnimationClip to the specified path
        AssetDatabase.CreateAsset(clip, "Assets/Exported/" + name + ".anim");
        AssetDatabase.SaveAssets();

        return clip;
    }

    public Animator SaveAnimatorController(string newPrefabPath)
    {
        // Get the RuntimeAnimatorController from the Animator
        RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;

        // Check if the RuntimeAnimatorController exists
        if (runtimeAnimatorController != null)
        {
            // Get the path of the original AnimatorController
            string originalPath = AssetDatabase.GetAssetPath(runtimeAnimatorController);

            // Create a new path for the duplicate AnimatorController
            string newPath = System.IO.Path.GetDirectoryName(newPrefabPath) + "/" + runtimeAnimatorController.name + "_copy.controller";

            // Create a copy of the AnimatorController
            AssetDatabase.CopyAsset(originalPath, newPath);

            // Assign the new AnimatorController to the Animator
            AnimatorController newAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(newPath);
            animator.runtimeAnimatorController = newAnimatorController;

          
        }
        
        return animator;
    }
}