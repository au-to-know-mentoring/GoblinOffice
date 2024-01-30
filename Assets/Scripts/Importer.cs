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
    public string xmlFileName = "PathToYourFileHere";
    public XDocument myXML;

    public float frameRate;

    //public GameObject EnemyTemplatePrefab;
    public Texture2D mySpriteSheet; //[TODO] Grab this info from the XML file.
    public SpriteSheetImporter mySpriteSheetImporter;
    public PrefabDuplicator myPrefabDuplicator;
    private RuntimeAnimatorController myAnimatorController;

    //public string NewAnimatorSavePath = "PathToSaveAnimatorHere";
    public Sprite[] sprites;

    private void Start()
    {
        myXML = XDocument.Load(xmlFileName);
        mySpriteSheetImporter = ImportFromXML(xmlFileName);

        if (sprites == null || sprites.Length < 1)
        {
            sprites = Resources.LoadAll<Sprite>("BogusSpriteSheet");
        }
        if(sprites.Length < 1)
        {
            Debug.LogError("Importer SpriteSheet not loaded correctly.");
        }
        CreateAnimationFromXML();
        //GenerateXMLTemplate();
        Debug.Log(mySpriteSheetImporter.EnemyName);
    }

    public SpriteSheetImporter ImportFromXML(string FilePath)
    {
        var serializer = new XmlSerializer(typeof(SpriteSheetImporter));
        using (var reader = new StreamReader(FilePath))
        {
            return (SpriteSheetImporter)serializer.Deserialize(reader);
        }
    }

    public void CreateAnimationFromXML()
    {
        // Load the XML document
        XmlDocument doc = new XmlDocument();
        doc.Load(xmlFileName);

        // Get all SpriteSheetImporter_AnimationClip elements
        XmlNodeList animationClipNodes = doc.GetElementsByTagName("SpriteSheetImporter_AnimationClip");
        myAnimatorController = myPrefabDuplicator.SaveAnimatorController("Assets/Exported/");
        foreach (XmlNode node in animationClipNodes)
        {
            // Get the name, starting frame, and ending frame
            string name = node.Attributes["Name"].Value;
            int startingFrame = int.Parse(node.Attributes["StartingFrame"].Value);
            int endingFrame = int.Parse(node.Attributes["EndingFrame"].Value);



            // Get the sprites for this animation clip from the sprite sheet
            //Sprite[] sprites = new Sprite[endingFrame - startingFrame + 1];



            //Array.Copy(sprites, startingFrame, sprites, 0, sprites.Length);

            // Create and save the animation clip
            //string path = $"Assets/Animations/{name}.anim";
            myPrefabDuplicator.CreateAnimationClip(sprites, startingFrame, endingFrame, frameRate, name);
            
        }
    }


    // ONLY use to regenerate XML Template
    private void GenerateXMLTemplate()
    {
        SpriteSheetImporter mySpriteSheetImporter = new SpriteSheetImporter();

        mySpriteSheetImporter.EnemyName = "GenericEnemyName";
        mySpriteSheetImporter.SpriteSheetFileName = "BogusSpriteSheetFileName";

        SpriteSheetImporter_AnimationClip myBogusIdleClip = new SpriteSheetImporter_AnimationClip();
        myBogusIdleClip.Name = "Idle";
        myBogusIdleClip.SpriteSheetIndex = 0;
        myBogusIdleClip.StartingFrame = 0;
        myBogusIdleClip.EndingFrame = 5;
        myBogusIdleClip.HasAnimationEvent = false;

        SpriteSheetImporter_AnimationClip myBogusDeathAnimClip = new SpriteSheetImporter_AnimationClip();
        myBogusDeathAnimClip.Name = "DeathAnim";
        myBogusDeathAnimClip.SpriteSheetIndex = 0;
        myBogusDeathAnimClip.StartingFrame = 0;
        myBogusDeathAnimClip.EndingFrame = 5;
        myBogusDeathAnimClip.HasAnimationEvent = false;

        SpriteSheetImporter_AnimationClip myBogusVulnerableAnim = new SpriteSheetImporter_AnimationClip();
        myBogusVulnerableAnim.Name = "VulnerableAnim";
        myBogusVulnerableAnim.SpriteSheetIndex = 0;
        myBogusVulnerableAnim.StartingFrame = 0;
        myBogusVulnerableAnim.EndingFrame = 5;
        myBogusVulnerableAnim.HasAnimationEvent = false;

        SpriteSheetImporter_AnimationClip myRangedAttackAnimator = new SpriteSheetImporter_AnimationClip();
        myRangedAttackAnimator.Name = "RangedAttack Animator";
        myRangedAttackAnimator.SpriteSheetIndex = 0;
        myRangedAttackAnimator.StartingFrame = 0;
        myRangedAttackAnimator.EndingFrame = 5;
        myRangedAttackAnimator.HasAnimationEvent = true;

        myRangedAttackAnimator.AnimationEventFrame = 3;
        myRangedAttackAnimator.AnimationEventFunctionName = "RangedAttackAnimationComplete";

        // Melee later

        mySpriteSheetImporter.myAnimationClips = new List<SpriteSheetImporter_AnimationClip>
        {
            //Add to list:
            myBogusIdleClip,
            myBogusDeathAnimClip,
            myBogusVulnerableAnim,
            myRangedAttackAnimator
        };

        var serializer = new XmlSerializer(typeof(SpriteSheetImporter));
        string path = "./assets/File.xml";

        var stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, mySpriteSheetImporter);
        stream.Close();

    }
}
[XmlRoot("SpriteSheetImporter")]
public class SpriteSheetImporter
{
    [XmlAttribute("EnemyName")]
    public string EnemyName;
    [XmlAttribute("SpriteSheetFileName")]
    public string SpriteSheetFileName;
    [XmlArray("MyAnimationClips"), XmlArrayItem("SpriteSheetImporter_AnimationClip")]
    public List<SpriteSheetImporter_AnimationClip> myAnimationClips;
    //List of SpriteSheets
    //projectile PNG?

}
[Serializable]
public class SpriteSheetImporter_AnimationClip
{
    // Clip settings
    [XmlAttribute("Name")]
    public string Name;
    [XmlAttribute("SpriteSheetIndex")]
    public int SpriteSheetIndex; // for multiple sprite sheets, and projectile.
    [XmlAttribute("StartingFrame")]
    public int StartingFrame;
    [XmlAttribute("EndingFrame")]
    public int EndingFrame;
    [XmlAttribute("HasAnimationEvent")]
    public bool HasAnimationEvent;
    // Animation Event settings
    [XmlAttribute("AnimationEventFrame")]
    public int AnimationEventFrame = -1; // treat negative numbers as null
    [XmlAttribute("AnimationEventFunctionName")]
    public string AnimationEventFunctionName = null;

}

public class SpriteSheet
{

}

