using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrefabDuplicator: MonoBehaviour
{
    public GameObject prefab; // The prefab to copy
    public Sprite[] oldSpriteSheet; // The original sprite sheet
    public Sprite[] newSpriteSheet; // The new sprite sheet
    public string newPrefabPath = "Assets/NewPrefab.prefab"; // The path to save the new prefab

    void Start()
    {
        // Instantiate a copy of the prefab
        GameObject newObject = Instantiate(prefab);

        // Get the Animator component
        Animator animator = newObject.GetComponent<Animator>();

        // Check if the Animator component exists
        if (animator != null)
        {
            // Get the AnimatorOverrideController from the Animator
            AnimatorOverrideController animatorOverrideController = animator.runtimeAnimatorController as AnimatorOverrideController;

            // Check if the AnimatorOverrideController exists
            if (animatorOverrideController != null)
            {
                // Get the current list of animation clips
                List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                animatorOverrideController.GetOverrides(overrides);

                // Loop through each animation clip
                for (int i = 0; i < overrides.Count; i++)
                {
                    // Get the current animation clip
                    AnimationClip clip = overrides[i].Value;

                    // Check if the animation clip exists
                    if (clip != null)
                    {
                        // Loop through each frame of the animation
                        foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                        {
                            // Check if the binding is a sprite
                            if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
                            {
                                // Get the current sprite curve
                                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);

                                // Loop through each keyframe
                                for (int j = 0; j < keyframes.Length; j++)
                                {
                                    // Get the current sprite
                                    Sprite sprite = keyframes[j].value as Sprite;

                                    // Check if the sprite exists
                                    if (sprite != null)
                                    {
                                        // Find the index of the sprite in the old sprite sheet
                                        int index = System.Array.IndexOf(oldSpriteSheet, sprite);

                                        // Check if the sprite was found in the old sprite sheet
                                        if (index != -1)
                                        {
                                            // Replace the sprite with the corresponding sprite from the new sprite sheet
                                            keyframes[j].value = newSpriteSheet[index];
                                        }
                                    }
                                }

                                // Set the new sprite curve
                                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
                            }
                        }
                    }
                }
            }
        }

        // Save the new object as a prefab
        PrefabUtility.SaveAsPrefabAsset(newObject, newPrefabPath);

        // Destroy the new object from the scene
        Destroy(newObject);
    }
}