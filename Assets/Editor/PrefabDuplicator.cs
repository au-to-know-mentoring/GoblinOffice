using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine.Playables;

public class PrefabDuplicator : MonoBehaviour
{
    public GameObject prefab; // The prefab to copy
    //public Sprite[] oldSpriteSheet; // The original sprite sheet
    //public Sprite[] newSpriteSheet; // The new sprite sheet
    public string newPrefabName = "Assets/NewPrefab.prefab"; //[TODO] GET FROM XML
    private RuntimeAnimatorController animatorController;
    private Animator animator;
    GameObject newObject;
    private string uniqueAssetsFolder = null;
    void Awake()
    {
        // Instantiate a copy of the prefab
        newObject = Instantiate(prefab);
        newObject.transform.position = new Vector3(2, 2, 0);

        // Get the Animator component
        animator = newObject.GetComponent<Animator>();
        animatorController = animator.runtimeAnimatorController;

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

        Keyframe[] myKeyFrames = new Keyframe[2];
        myKeyFrames[0] = new Keyframe(0.0f, 1.0f);
        float testa = (endFrame - startFrame) / frameRate;
        testa -= (1 / frameRate);
        myKeyFrames[1] = new Keyframe(testa, 1f);
        AnimationCurve myCurve = new AnimationCurve(myKeyFrames);
        AnimationClipSettings mySettings = new AnimationClipSettings();
        
        if(clip)
        mySettings.loopTime = true;

   
        AnimationUtility.SetAnimationClipSettings(clip, mySettings);
        clip.SetCurve("", typeof(Transform), "localScale.x", myCurve);
        
        AssetDatabase.Refresh();



        // Ensure a unique asset folder is available based on the prefab name
        string uniqueFolderPath = EnsureUniqueAssetFolder("Assets/ExportedAnimations", name);
        string animationName = name; // Customize this name as needed
        string uniquePath = Path.Combine(uniqueFolderPath, animationName + ".anim");
        
        //RemoveAllScaleCurves(clip); Doesn't work but makes no errors.
        
        // Save the AnimationClip to the specified path
        AssetDatabase.CreateAsset(clip, uniquePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Animation: " + clip.name + " saved to " + uniquePath);


        


        return clip;
    }

    public RuntimeAnimatorController SaveAnimatorController(string newPrefabPath, string enemyName) // + string name)
    {
        // Get the RuntimeAnimatorController from the Animator
        //RuntimeAnimatorController runtimeAnimatorController = animatorController.runtimeAnimatorController;

        // Check if the RuntimeAnimatorController exists
        if (animatorController != null)
        {
            // Get the path of the original AnimatorController
            string originalPath = AssetDatabase.GetAssetPath(animatorController);

            // Ensure a unique asset folder is available based on the enemy name
            string baseDirectory = Path.GetDirectoryName(newPrefabPath);
            string uniqueFolderPath = EnsureUniqueAssetFolder(baseDirectory, enemyName);
            string newPath = Path.Combine(uniqueFolderPath, enemyName + ".controller");

            AssetDatabase.CopyAsset(originalPath, newPath);

            // Assign the new AnimatorController to the Animator
            AnimatorController newAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(newPath);
            //animatorController.runtimeAnimatorController = newAnimatorController;
        }
        else
        {
            Debug.LogError("Animator for Duplicator NULL");
        }
        // Save the new object as a prefab in the same directory
        string prefabPath = System.IO.Path.GetDirectoryName(newPrefabPath) + "/" + enemyName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(newObject, prefabPath);

        return animatorController;
    }

    private string EnsureUniqueAssetFolder(string baseDirectory, string prefabName)
    {
        // If we've already determined a unique folder for this operation, use it
        if (!string.IsNullOrEmpty(uniqueAssetsFolder))
        {
            return uniqueAssetsFolder;
        }

        string folderName = prefabName;
        int counter = 0;

        // Check if the folder already exists; if so, append a counter to the name until a unique name is found
        while (AssetDatabase.IsValidFolder(Path.Combine(baseDirectory, folderName)))
        {
            counter++;
            folderName = $"{prefabName}({counter})";
        }

        // Create the new directory
        string newDirectoryPath = Path.Combine(baseDirectory, folderName);
        AssetDatabase.CreateFolder(baseDirectory, folderName);

        // Store this directory path for reuse during this operation
        uniqueAssetsFolder = newDirectoryPath;

        return newDirectoryPath;
    }

    public void RemoveLocalScaleXCurve(AnimationClip clip)      // Doesn't work
    {
        // Define the curve binding for localScale.x
        EditorCurveBinding curveBinding = EditorCurveBinding.FloatCurve("", typeof(Transform), "localScale.x");

        // Remove the curve by setting it to null
        AnimationUtility.SetEditorCurve(clip, curveBinding, null);

        Debug.Log("Removed localScale.x curve from " + clip.name);
    }

    public void RemoveAllScaleCurves(AnimationClip clip)        // Doesn't work
    {
        // Define the curve bindings for localScale.x, localScale.y, and localScale.z
        EditorCurveBinding scaleXBinding = EditorCurveBinding.FloatCurve("", typeof(Transform), "localScale.x");
        EditorCurveBinding scaleYBinding = EditorCurveBinding.FloatCurve("", typeof(Transform), "localScale.y");
        EditorCurveBinding scaleZBinding = EditorCurveBinding.FloatCurve("", typeof(Transform), "localScale.z");

        // Remove the curves by setting them to null
        AnimationUtility.SetEditorCurve(clip, scaleXBinding, null);
        AnimationUtility.SetEditorCurve(clip, scaleYBinding, null);
        AnimationUtility.SetEditorCurve(clip, scaleZBinding, null);

        Debug.Log("Removed all localScale curves from " + clip.name);
    }
}