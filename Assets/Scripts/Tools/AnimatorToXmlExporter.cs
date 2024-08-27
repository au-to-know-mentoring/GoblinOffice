using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorToXMLExporter : MonoBehaviour
{
    public AnimatorController animatorController; // Drag and drop the AnimatorController here
    public string outputXmlPath = "Assets/AnimatorExport.xml";

    [ContextMenu("Export Animator to XML")]
    public void ExportAnimatorToXML()
    {
        if (animatorController == null)
        {
            Debug.LogError("Animator Controller not assigned.");
            return;
        }

        XElement root = new XElement("SpriteSheetImporter",
            new XAttribute("EnemyName", "GenericEnemyName"),
            new XAttribute("SpriteSheetFileName", "trollspritetestphotoshop"));

        XElement parametersElement = new XElement("Parameters");
        foreach (var param in animatorController.parameters)
        {
            parametersElement.Add(new XElement("Parameter",
                new XAttribute("Name", param.name),
                new XAttribute("Type", param.type.ToString())));
        }
        root.Add(parametersElement);

        XElement statesElement = new XElement("States");
        foreach (var layer in animatorController.layers)
        {
            foreach (var state in layer.stateMachine.states)
            {
                XElement stateElement = new XElement("State",
                    new XAttribute("Name", state.state.name),
                    new XAttribute("StartingFrame", 0), // Placeholder, adjust as needed
                    new XAttribute("EndingFrame", 0)); // Placeholder, adjust as needed

                XElement transitionsElement = new XElement("Transitions");
                foreach (var transition in state.state.transitions)
                {
                    XElement transitionElement = new XElement("Transition",
                        new XAttribute("To", transition.destinationState.name));

                    if (transition.conditions.Length > 0)
                    {
                        transitionElement.Add(new XAttribute("Condition", transition.conditions[0].parameter));
                    }
                    else
                    {
                        transitionElement.Add(new XAttribute("Condition", "None"));
                    }

                    transitionsElement.Add(transitionElement);
                }
                stateElement.Add(transitionsElement);
                statesElement.Add(stateElement);
            }
        }
        root.Add(statesElement);

        XDocument doc = new XDocument(root);
        doc.Save(outputXmlPath);

        Debug.Log("Animator exported to XML at: " + outputXmlPath);
    }
}