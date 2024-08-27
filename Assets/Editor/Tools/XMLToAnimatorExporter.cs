using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class XMLToAnimatorImporter : MonoBehaviour
{
    public TextAsset xmlFile; // Change from string to TextAsset
    public string animatorControllerPath = "Assets/MyAnimatorController.controller";

    private string uniqueAssetsFolder = null;

    [ContextMenu("Import Animator from XML")]
    public void ImportAnimatorFromXML()
    {
        if (xmlFile == null)
        {
            Debug.LogError("XML file is not assigned.");
            return;
        }

        XDocument doc = XDocument.Parse(xmlFile.text);
        string spriteSheetFileName = doc.Root.Attribute("SpriteSheetFileName")?.Value;
        if (string.IsNullOrEmpty(spriteSheetFileName) || Resources.Load<Texture2D>(spriteSheetFileName) == null)
        {
            Debug.LogError("Spritesheet not found: " + spriteSheetFileName);
            return;
        }

        Sprite[] sprites = Resources.LoadAll<Sprite>(spriteSheetFileName);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("No sprites loaded from spritesheet: " + spriteSheetFileName);
            return;
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(animatorControllerPath);
        if (controller == null)
        {
            Debug.LogError("Failed to create Animator Controller at specified path.");
            return;
        }

        // Ensure there is at least one layer
        if (controller.layers.Length == 0)
        {
            controller.AddLayer("Base Layer");
        }

        // Add parameters
        var parameters = doc.Root.Element("Parameters")?.Elements("Parameter");
        if (parameters == null)
        {
            Debug.LogError("No parameters found in XML.");
            return;
        }

        foreach (var param in parameters)
        {
            var paramName = param.Attribute("Name")?.Value;
            var paramType = param.Attribute("Type")?.Value;
            if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(paramType))
            {
                Debug.LogError("Parameter name or type is missing.");
                continue;
            }

            switch (paramType)
            {
                case "Float":
                    controller.AddParameter(paramName, AnimatorControllerParameterType.Float);
                    break;
                case "Bool":
                    controller.AddParameter(paramName, AnimatorControllerParameterType.Bool);
                    break;
                case "Trigger":
                    controller.AddParameter(paramName, AnimatorControllerParameterType.Trigger);
                    break;
                case "Int":
                    controller.AddParameter(paramName, AnimatorControllerParameterType.Int);
                    break;
                default:
                    Debug.LogError($"Unknown parameter type: {paramType}");
                    break;
            }
        }

        // Dictionary to hold state names and their corresponding animator states
        Dictionary<string, AnimatorState> animatorStates = new Dictionary<string, AnimatorState>();

        // Add states and animations
        var states = doc.Root.Element("States")?.Elements("State");
        if (states == null)
        {
            Debug.LogError("No states found in XML.");
            return;
        }

        foreach (var state in states)
        {
            var stateName = state.Attribute("Name")?.Value;
            var startFrame = state.Attribute("StartingFrame")?.Value;
            var endFrame = state.Attribute("EndingFrame")?.Value;

            if (string.IsNullOrEmpty(stateName) || string.IsNullOrEmpty(startFrame) || string.IsNullOrEmpty(endFrame))
            {
                Debug.LogError("State name or frame range is missing.");
                continue;
            }

            AnimationClip clip = new AnimationClip();
            clip.frameRate = 10; // Set frame rate (10 fps for example)

            // Create keyframes for sprite animation
            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[int.Parse(endFrame) - int.Parse(startFrame) + 1];
            for (int i = int.Parse(startFrame); i <= int.Parse(endFrame); i++)
            {
                keyFrames[i - int.Parse(startFrame)] = new ObjectReferenceKeyframe
                {
                    time = (i - int.Parse(startFrame)) / clip.frameRate,
                    value = sprites[i]
                };
            }

            // Apply the keyframes to the animation clip
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keyFrames);

            var animatorState = controller.layers[0].stateMachine.AddState(stateName);
            animatorState.motion = clip;

            // Add the state to the dictionary
            animatorStates[stateName] = animatorState;
        }

        // Add transitions based on XML
        foreach (var state in states)
        {
            var stateName = state.Attribute("Name")?.Value;
            if (string.IsNullOrEmpty(stateName) || !animatorStates.TryGetValue(stateName, out var animatorState))
            {
                Debug.LogError($"State '{stateName}' not found in animator states dictionary.");
                continue;
            }

            var transitions = state.Element("Transitions")?.Elements("Transition");
            if (transitions == null)
            {
                Debug.LogWarning($"No transitions found for state '{stateName}'.");
                continue;
            }

            foreach (var transition in transitions)
            {
                var toState = transition.Attribute("To")?.Value;
                var condition = transition.Attribute("Condition")?.Value;

                if (string.IsNullOrEmpty(toState) || string.IsNullOrEmpty(condition))
                {
                    Debug.LogError("Transition target state or condition is missing.");
                    continue;
                }

                if (animatorStates.TryGetValue(toState, out var targetState))
                {
                    var animatorTransition = animatorState.AddTransition(targetState);
                    animatorTransition.AddCondition(AnimatorConditionMode.If, 0, condition);
                }
                else
                {
                    Debug.LogError($"Target state '{toState}' not found for transition from state '{stateName}'.");
                }
            }
        }

        Debug.Log("Animator imported from XML successfully.");
    }



    public AnimationClip CreateAnimationClip(Sprite[] sprites, int startFrame, int endFrame, float frameRate, string name)
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
        myKeyFrames[1] = new Keyframe((endFrame - startFrame) / frameRate - 1, 1f);
        AnimationCurve myCurve = new AnimationCurve(myKeyFrames);
        AnimationClipSettings mySettings = new AnimationClipSettings();

        if (clip)
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
}

