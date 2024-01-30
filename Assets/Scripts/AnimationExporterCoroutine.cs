//using System.Xml.Serialization;
//using UnityEditor;
//using UnityEngine.Playables;
//using UnityEngine;
//using System.Collections.Generic;
//using System.IO;
//using System.Collections;

//public class AnimationExporterCoroutine : MonoBehaviour
//{
//    // Declare instance variables
//    private GameObject selectedObject;
//    private Animator animator;
//    private AnimationData animationData;
//    private SpriteRenderer spriteRenderer;
//    private AnimationClip myClip;
//    private int frameIndex;
//    private int animationIndex;
//    private int totalAnimations;
//    private int totalFrames;

//    void Start()
//    {
//        // Initialize instance variables
//        selectedObject = Selection.activeGameObject;
//        animator = selectedObject.GetComponent<Animator>();
//        animator.speed = 0f;
//        animationData = new AnimationData();
//        spriteRenderer = selectedObject.GetComponent<SpriteRenderer>();

//        // Get the number of animations
//        totalAnimations = animator.runtimeAnimatorController.animationClips.Length;

//        // Start the ExportAnimation coroutine
//        StartCoroutine(ExportAnimation());
//    }

//    IEnumerator ExportAnimation()
//    {
//        yield return new WaitForSeconds(0.1f);
//        for (animationIndex = 0; animationIndex < totalAnimations; animationIndex++)
//        {
//            myClip = animator.runtimeAnimatorController.animationClips[animationIndex];
//            float frames = myClip.frameRate * myClip.length;
//            totalFrames = Mathf.RoundToInt(frames);
//            yield return new WaitForSeconds(0.1f);
//            for (frameIndex = 0; frameIndex < totalFrames; frameIndex++)
//            {
//                // Set the animator to the current frame
//                float normalizedTime = (float)frameIndex / totalFrames;
//                animator.Play(myClip.name, 0, normalizedTime);

//                // Wait for a small delay to give the Animator time to update
//                yield return new WaitForSeconds(0.1f);

//                // Get the sprite name from the SpriteRenderer
//                string spriteName = spriteRenderer.sprite.name;

//                // Populate frame data
//                FrameData frameData = new FrameData();
//                frameData.AnimationName = myClip.name;
//                frameData.SpriteName = spriteName ?? "UnknownSprite";
//                frameData.FrameIndex = frameIndex;

//                // Add the frame data to the animation data
//                animationData.Frames.Add(frameData);
//            }
//        }

//        XmlSerializer serializer = new XmlSerializer(typeof(AnimationData));
//        using (FileStream stream = new FileStream("Assets/animationCoroutine_data.xml", FileMode.Create))
//        {
//            serializer.Serialize(stream, animationData);
//        }

//        Debug.Log("Animation exported to XML: Assets/animationCoroutine_data.xml");
//    }

//    [System.Serializable]
//    public class AnimationData
//    {
//        public List<FrameData> Frames = new List<FrameData>();
//    }

//    [System.Serializable]
//    public class FrameData
//    {
//        public string AnimationName; // Generate based of enemy name, Declare start and end frame
//                                     //public Vector2 Size;
//        public string SpriteName;
//        public int FrameIndex;
//        // Add more properties as needed
//    }
//}
