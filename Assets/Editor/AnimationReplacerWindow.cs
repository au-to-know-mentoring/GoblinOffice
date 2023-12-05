//using UnityEngine;
//using UnityEditor;

//public class AnimationReplacerWindow : EditorWindow
//{
//    private GameObject prefab;
//    private Sprite[] newSprites;

//    [MenuItem("Tools/Animation Replacer")]
//    public static void ShowWindow()
//    {
//        GetWindow<AnimationReplacerWindow>("Animation Replacer");
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label("Animation Replacer", EditorStyles.boldLabel);

//        prefab = EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false) as GameObject;
//        newSprites = EditorGUILayout.ObjectField("New Spritesheet", newSprites, typeof(Sprite[]), false) as Sprite[];

//        if (GUILayout.Button("Replace Animations"))
//        {
//            ReplaceAnimations();
//        }
//    }

//    private void ReplaceAnimations()
//    {
//        if (prefab == null || newSprites == null || newSprites.Length == 0)
//        {
//            Debug.LogError("Prefab and new spritesheet must be set.");
//            return;
//        }

//        Animator animator = prefab.GetComponent<Animator>();

//        if (animator == null)
//        {
//            Debug.LogError("Prefab must have an Animator component.");
//            return;
//        }

//        AnimationClip[] animationClips = AnimationUtility.GetAnimationClips(prefab);

//        foreach (AnimationClip clip in animationClips)
//        {
//            EditorCurveBinding[] spriteBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

//            foreach (EditorCurveBinding binding in spriteBindings)
//            {
//                if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
//                {
//                    ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);

//                    for (int i = 0; i < Mathf.Min(keyframes.Length, newSprites.Length); i++)
//                    {
//                        keyframes[i].value = newSprites[i];
//                    }

//                    AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
//                }
//            }
//        }
//    }
//}
