using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;

public class AnimationReplacer : EditorWindow
{
    private AnimatorController controller;
    private GameObject prefab;
    private Dictionary<string, AnimationClip> newClips = new Dictionary<string, AnimationClip>();

    [MenuItem("Window/Animation Replacer")]
    public static void ShowWindow()
    {
        GetWindow<AnimationReplacer>("Animation Replacer");
    }

    private void OnGUI()
    {
        controller = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", controller, typeof(AnimatorController), false);
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

        // Modified prefab field to detect changes
        EditorGUI.BeginChangeCheck(); // Start checking for changes
        if (EditorGUI.EndChangeCheck()) // Check if the prefab field has changed
        {
            SetAnimatorFromPrefab(); // Call SetAnimatorFromPrefab if there's a change
        }

        if (controller != null)
        {
            string controllerPath = AssetDatabase.GetAssetPath(controller);
            string directory = System.IO.Path.GetDirectoryName(controllerPath);

            // Find all AnimationClip assets in the same directory as the controller
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { directory });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (!newClips.ContainsKey(clip.name))
                {
                    newClips[clip.name] = clip;
                }
            }
            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    if (!newClips.ContainsKey(state.state.name))
                    {
                        newClips[state.state.name] = state.state.motion as AnimationClip;
                    }

                    newClips[state.state.name] = (AnimationClip)EditorGUILayout.ObjectField(state.state.name, newClips[state.state.name], typeof(AnimationClip), false);
                }
            }

            if (GUILayout.Button("Replace Animations"))
            {
                // Create a copy of the AnimatorController
                AnimatorController newController = Instantiate(controller);
                newController.AddLayer("New Layer");
                foreach (var layer in newController.layers)
                {
                    foreach (var state in layer.stateMachine.states)
                    {
                        if (newClips.ContainsKey(state.state.name) && newClips[state.state.name] != null)
                        {
                            state.state.motion = newClips[state.state.name];
                        }
                    }
                }

                SaveAndCopy(newController);
            }

            if (GUILayout.Button("Replace Animations with Prefab Path"))
            {
                AnimatorController newController = Instantiate(controller);
                ReplaceAnimationsWithPrefabPath();
                SaveAndCopy(newController);
            }
        }

        void SaveAndCopy(AnimatorController newController)
        {
            // Save the new AnimatorController
            string path = "Assets/Exported/" + controller.name + "_REPLACED.controller";
            AssetDatabase.CreateAsset(newController, path);
            AssetDatabase.SaveAssets();

            // Create a copy of the prefab
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            newPrefab.GetComponent<Animator>().runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            // Save the new prefab
            string prefabPath = "Assets/Exported/" + prefab.name + "_REPLACED.prefab";
            PrefabUtility.SaveAsPrefabAsset(newPrefab, prefabPath);

            // Clean up
            DestroyImmediate(newPrefab);
            newClips.Clear();
        }
    }


    public void SetAnimatorFromPrefab()
    {
        if (prefab != null)
        {
            Animator foundAnimator = prefab.GetComponentInChildren<Animator>();
            if (foundAnimator != null)
            {
                controller = foundAnimator.runtimeAnimatorController as AnimatorController;
            }
        }
    }

    private void SetAnimatorControllerFromPrefabPath()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is not set.");
            return;
        }

        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        string directory = System.IO.Path.GetDirectoryName(prefabPath);

        // Find all Animator components in the same directory as the prefab
        string[] guids = AssetDatabase.FindAssets("t:Animator", new[] { directory });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Animator animator = go.GetComponent<Animator>();
            if (animator != null)
            {
                controller = animator.runtimeAnimatorController as AnimatorController;
                break; // Assuming you want to set the first found AnimatorController
            }
        }

        if (controller == null)
        {
            Debug.LogError("No AnimatorController found in the same path as the prefab.");
        }
    }

    public void ReplaceAnimationsWithPrefabPath()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is not set.");
            return;
        }

        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        string directory = System.IO.Path.GetDirectoryName(prefabPath);

        // Find all AnimationClip assets in the same directory AND SUBDIRECTORIES!!! of prefab.
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { directory });
        Dictionary<string, AnimationClip> prefabClips = new Dictionary<string, AnimationClip>();
        foreach (string guid in guids) // GUID = (Globally Unique Identifiers) representing assets for Unity.
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (!prefabClips.ContainsKey(clip.name))
            {
                prefabClips[clip.name] = clip;
            }
        }

        if (controller == null)
        {
            Debug.LogError("Animator Controller is not set.");
            return;
        }

        // Replace animations in the AnimatorController with the found clips
        foreach (var layer in controller.layers)
        {
            foreach (var state in layer.stateMachine.states)
            {
                if (state.state.motion is AnimationClip motionClip)
                {
                    // Extract the AnimationName part from the motionClip's name
                    string motionClipAnimationName = motionClip.name.Substring(motionClip.name.IndexOf('_') + 1);

                    AnimationClip matchingClip = null;
                    // Loop through prefabClips to find a matching clip by AnimationName
                    foreach (var entry in prefabClips) // death,idle,ranged,vuln
                    {
                        // Extract the AnimationName part from the entry's key
                        string entryAnimationName = entry.Key.Substring(entry.Key.IndexOf('_') + 1);
                        if (entryAnimationName == motionClipAnimationName)
                        {
                            matchingClip = entry.Value;
                            break; // Stop searching once a match is found
                        }
                    }

                    if (matchingClip != null)
                    {
                        // Found a matching animation clip by AnimationName
                        // You can now replace the state's motion with matchingClip or perform other actions
                        state.state.motion = matchingClip;
                    }
                }
            }
        }

        Debug.Log("Animations replaced with .anim files from the same path as the prefab.");
    }


}