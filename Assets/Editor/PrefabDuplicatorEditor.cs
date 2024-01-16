//using UnityEngine;
//using UnityEditor;
//using UnityEditor.Animations;
//using System.Collections.Generic;
//using System.IO;

//public class PrefabDuplicatorEditor : EditorWindow
//{
//    GameObject prefab;
//    Sprite[] oldSpriteSheet;
//    Sprite[] newSpriteSheet;
//    string newPrefabPath = "Assets/NewPrefab.prefab";

//    [MenuItem("Window/Prefab Duplicator")]
//    public static void ShowWindow()
//    {
//        GetWindow<PrefabDuplicatorEditor>("Prefab Duplicator");
//    }

//    void OnGUI()
//    {
//        GUILayout.Label("Prefab Duplicator", EditorStyles.boldLabel);

//        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
//        oldSpriteSheet = GetArrayField("Old Sprite Sheet", oldSpriteSheet);
//        newSpriteSheet = GetArrayField("New Sprite Sheet", newSpriteSheet);
//        newPrefabPath = EditorGUILayout.TextField("New Prefab Path", newPrefabPath);

//        if (GUILayout.Button("Duplicate Prefab"))
//        {
//            DuplicatePrefab();
//        }
//    }

//    Sprite[] GetArrayField(string name, Sprite[] array)
//    {
//        GUILayout.BeginVertical();

//        if (GUILayout.Button("Load " + name))
//        {
//            string path = EditorUtility.OpenFolderPanel("Load Sprites from Folder", "", "");
//            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);

//            string[] files = AssetDatabase.FindAssets("t:Sprite", new string[] { relativePath });
//            array = new Sprite[files.Length];

//            for (int i = 0; i < files.Length; i++)
//            {
//                string assetPath = AssetDatabase.GUIDToAssetPath(files[i]);
//                array[i] = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
//            }
//        }

//        for (int i = 0; i < array.Length; i++)
//        {
//            EditorGUILayout.ObjectField(name + " Element " + i, array[i], typeof(Sprite), false);
//        }

//        GUILayout.EndVertical();

//        return array;
//    }

//    void DuplicatePrefab()
//    {
//        if (prefab == null)
//        {
//            Debug.LogError("Prefab is not set.");
//            return;
//        }

//        // Instantiate a copy of the prefab
//        GameObject newObject = Instantiate(prefab);

//        // Get the Animator component
//        Animator animator = newObject.GetComponent<Animator>();

//        // Check if the Animator component exists
//        if (animator != null)
//        {
//            // Get the RuntimeAnimatorController from the Animator
//            RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;

//            // Check if the RuntimeAnimatorController exists
//            if (runtimeAnimatorController != null)
//            {
//                // Get the path of the original AnimatorController
//                string originalPath = AssetDatabase.GetAssetPath(runtimeAnimatorController);

//                // Create a new path for the duplicate AnimatorController
//                string newPath = "Assets/Exported/" + runtimeAnimatorController.name + "_copy.controller";

//                // Create a copy of the AnimatorController
//                AssetDatabase.CopyAsset(originalPath, newPath);

//                // Assign the new AnimatorController to the Animator
//                AnimatorController newAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(newPath);
//                animator.runtimeAnimatorController = newAnimatorController;

//                // Iterate over the layers
//                for (int i = 0; i < newAnimatorController.layers.Length; i++)
//                {
//                    var layer = newAnimatorController.layers[i];
//                    var stateMachine = layer.stateMachine;

//                    // Iterate over the states
//                    for (int j = 0; j < stateMachine.states.Length; j++)
//                    {
//                        var state = stateMachine.states[j].state;

//                        // Check if the state's motion is an AnimationClip
//                        if (state.motion is AnimationClip oldClip)
//                        {
//                            // Get the path of the original AnimationClip
//                            string originalClipPath = AssetDatabase.GetAssetPath(oldClip);

//                            // Create a new path for the duplicate AnimationClip
//                            string newClipPath = "Assets/Exported/" + oldClip.name + "_copy.anim";

//                            // Create a copy of the AnimationClip
//                            AssetDatabase.CopyAsset(originalClipPath, newClipPath);

//                            // Load the new AnimationClip
//                            AnimationClip newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(newClipPath);

//                            // Replace the old AnimationClip with the new one in the state
//                            state.motion = newClip;
//                        }
//                    }
//                }
//            }
//        }

//        // Save the new object as a prefab
//        string newPrefabPath = "Assets/Exported/" + prefab.name + "_copy.prefab";
//        PrefabUtility.SaveAsPrefabAssetAndConnect(newObject, newPrefabPath, InteractionMode.UserAction);

//        // Destroy the new object from the scene
//        DestroyImmediate(newObject);
//    }
//}
