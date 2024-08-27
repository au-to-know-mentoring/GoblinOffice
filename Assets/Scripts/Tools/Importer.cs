using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine;
using System.Xml.Serialization;
using System;
using System.Xml.Linq;
using UnityEditor;
using System.Linq;
using UnityEditor.Animations;

public class Importer : MonoBehaviour
{
    public TextAsset xmlFile; // Changed from string xmlFileName
    public SpriteSheetImporter mySpriteSheetImporter;
    public Sprite[] sprites;
    public string exportFolderPath = "Assets/test"; // Default export path

    private void Start()
    {
        if (xmlFile == null)
        {
            Debug.LogError("XML file is not assigned.");
            return;
        }

        mySpriteSheetImporter = ImportFromXML(xmlFile);

        if (sprites == null || sprites.Length < 1)
        {
            // Load spritesheet by xml here
            sprites = Resources.LoadAll<Sprite>(mySpriteSheetImporter.SpriteSheetFileName);
        }
        if (sprites.Length < 1)
        {
            Debug.LogError("Importer SpriteSheet not loaded correctly.");
        }
        CreateAnimationControllerFromXML(xmlFile);
        Debug.Log(mySpriteSheetImporter.EnemyName);
    }

    public SpriteSheetImporter ImportFromXML(TextAsset xmlAsset)
    {
        var serializer = new XmlSerializer(typeof(SpriteSheetImporter));
        using (var reader = new StringReader(xmlAsset.text))
        {
            return (SpriteSheetImporter)serializer.Deserialize(reader);
        }
    }

    public void CreateAnimationControllerFromXML(TextAsset xmlAsset)
    {
        XDocument doc = XDocument.Parse(xmlAsset.text);
        string folderPath = string.IsNullOrEmpty(exportFolderPath) ? "Assets/test" : exportFolderPath;

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", folderPath.Substring(7)); // Assuming folderPath starts with "Assets/"
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath($"{folderPath}/MyAnimatorController.controller");
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

            AnimatorControllerParameterType type = AnimatorControllerParameterType.Float; // Default
            switch (paramType)
            {
                case "Float":
                    type = AnimatorControllerParameterType.Float;
                    break;
                case "Bool":
                    type = AnimatorControllerParameterType.Bool;
                    break;
                case "Trigger":
                    type = AnimatorControllerParameterType.Trigger;
                    break;
                case "Int":
                    type = AnimatorControllerParameterType.Int;
                    break;
                default:
                    Debug.LogError($"Unknown parameter type: {paramType}");
                    continue;
            }
            controller.AddParameter(paramName, type);
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
            var sampleRate = state.Attribute("SampleRate")?.Value ?? "12"; // Default to 12 if sample rate is not provided

            if (string.IsNullOrEmpty(stateName) || string.IsNullOrEmpty(startFrame) || string.IsNullOrEmpty(endFrame) || string.IsNullOrEmpty(sampleRate))
            {
                Debug.LogError("State name, frame range, or sample rate is missing.");
                continue;
            }

            AnimationClip clip = new AnimationClip();
            clip.frameRate = float.Parse(sampleRate); // Set frame rate from XML

            EditorCurveBinding curveBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            int startFrameIndex = int.Parse(startFrame);
            int endFrameIndex = int.Parse(endFrame);
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[endFrameIndex - startFrameIndex + 1];

            for (int i = startFrameIndex; i <= endFrameIndex; i++)
            {
                keyframes[i - startFrameIndex] = new ObjectReferenceKeyframe
                {
                    time = (i - startFrameIndex) / clip.frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyframes);
            AssetDatabase.CreateAsset(clip, $"{folderPath}/{stateName}.anim");

            AnimatorState animatorState = controller.layers[0].stateMachine.AddState(stateName);
            animatorState.motion = clip;

            animatorStates[stateName] = animatorState;
        }

        // Add transitions
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
            //foreach (var transition in transitions)
            //{
            //    var toState = transition.Attribute("To")?.Value;
            //    var condition = transition.Attribute("Condition")?.Value;

            //    if (string.IsNullOrEmpty(toState) || string.IsNullOrEmpty(condition))
            //    {
            //        Debug.LogError("Transition target state or condition is missing.");
            //        continue;
            //    }

            //    if (animatorStates.TryGetValue(toState, out var targetState))
            //    {
            //        var animatorTransition = animatorState.AddTransition(targetState);
            //        string[] conditionParts = condition.Split(' ');
            //        if (conditionParts.Length == 3 && int.TryParse(conditionParts[2], out int conditionValue))
            //        {
            //            animatorTransition.AddCondition(AnimatorConditionMode.Equals, conditionValue, conditionParts[0]);
            //        }
            //        else
            //        {
            //            Debug.LogError("Invalid transition condition format.");
            //        }
            //    }
            //    else
            //    {
            //        Debug.LogError($"Target state '{toState}' not found for transition from state '{stateName}'.");
            //    }
            //}
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CreatePrefabWithAnimator(controller, folderPath);
    }

    private void CreatePrefabWithAnimator(AnimatorController controller, string folderPath)
    {
        GameObject newGameObject = new GameObject("AnimatedGameObject");

        // Add a SpriteRenderer component
        SpriteRenderer spriteRenderer = newGameObject.AddComponent<SpriteRenderer>();
        if (sprites != null && sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[0]; // Optionally set the first sprite as default
        }

        // Add an Animator component
        Animator animator = newGameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        string prefabPath = $"{folderPath}/AnimatedGameObject.prefab";
        PrefabUtility.SaveAsPrefabAsset(newGameObject, prefabPath);
        DestroyImmediate(newGameObject); // Clean up the temporary GameObject

        Debug.Log($"Prefab created at: {prefabPath}");
    }
}

[XmlRoot("SpriteSheetImporter")]
public class SpriteSheetImporter
{
    [XmlAttribute("EnemyName")]
    public string EnemyName;
    [XmlAttribute("SpriteSheetFileName")]
    public string SpriteSheetFileName;
    [XmlArray("Parameters"), XmlArrayItem("Parameter")]
    public List<Parameter> Parameters;
    [XmlArray("States"), XmlArrayItem("State")]
    public List<State> States;
}

public class Parameter
{
    [XmlAttribute("Name")]
    public string Name;
    [XmlAttribute("Type")]
    public string Type;
}

public class State
{
    [XmlAttribute("Name")]
    public string Name;
    [XmlAttribute("StartingFrame")]
    public int StartingFrame;
    [XmlAttribute("EndingFrame")]
    public int EndingFrame;
    [XmlAttribute("SampleRate")]
    public int SampleRate;
    [XmlArray("Transitions"), XmlArrayItem("Transition")]
    public List<Transition> Transitions;
}

public class Transition
{
    [XmlAttribute("To")]
    public string To;
    [XmlAttribute("Condition")]
    public string Condition;
}
