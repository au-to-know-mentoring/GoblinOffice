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

        if (controller != null)
        {
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
    }
}