using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System;

public class PathfindingObject : MonoBehaviour
{

    public Vector3Int startPos;
    public Vector3Int endPos;
    public float TimeBetweenTiles = 1f;
    public float arrivalTime = 5f; // dont use in movement logic besides start time (DONE)
    //private TileBase[] obstacleTiles; // this isn't currently used.
    private Dictionary<Vector3Int, PathfindingManager.Node> nodeDictionary;
    public bool UpdateStartingPosition = true;
    [Header("Attack Variables")]
    public bool MeleeMode;
    public GameObject rangedAttack;
    public int rangedAttackQuantity;
    public int rangedAttackQuantityOriginal;
    public float delayBetweenRangedAttacks; // Change to beat manager eventually
    public float RangedAttackAnimationTime; // TODO
    public float MeleeAttackAnimationTime;
    private float rangedAttackDelayTimer = 0;
    private RangedProjectile rangedAttackedScript;
    public float projectileSpeed;
    //public List<string> searchTerms = new List<string> { "Ranged", "Melee", "Death" };
    public List<RangedBeat> rangedAttacksList = new List<RangedBeat>();
    public List<RangedBeat> OriginalAttackList = new List<RangedBeat>();
    public List<RangedBeat> ItemsToRemove = new List<RangedBeat>();
    public List<String> StringListOfActions = new List<String>();
    [Header("Vulnerable Settings.")]
    public int VulnerableBeat = 999;
    public float VulnerableDuration = 1.99f;
    public bool isVulnerable = false;
    public bool Active = false;
    public bool Dead = false;


    private List<PathfindingManager.Node> currentPath;
    [SerializeField]
    private float TimeToStart;
    private float journeyLength;
    private Vector3 offset;
    [Header("View only Variables")]
    [SerializeField]
    private Vector3 targetPosition;
    [SerializeField]
    [Tooltip("Speed is set by 1 / TimeBetweenTiles")]
    private float speed = 5f;
    [SerializeField]
    private int targetIndex = 0;
    [SerializeField]
    private Tilemap obstacleTilemap;

    [SerializeField]
    private float myTimer;


    private Animator myAnimator;

    private bool Assigned;

    //private float attackTime;
    //private float damageTime;
    //private float deathTime;
    //private float idleTime;
    public AnimationSettings myAnimationSettings;

    //private AnimationClip clip;

    public InputManager myInputManager;
    /// </summary>
    /// <param name="obstacleTilemap"></param>
    public Color myColour = Color.Debug0;
    public SettingsData GlobalSettingsObject;
    public TimeManager GlobalTimeManager;
    public enum Color
    {
        Debug0,
        Green1,
        Red2,
        Blue3,
        Yellow4
    }
    public void setObstacleTilemap(Tilemap obstacleTilemap)
    {
        this.obstacleTilemap = obstacleTilemap;

    }

    public void ResetVulnerableBeat()
    {
        VulnerableBeat = 999;
    }
    private void Start()
    {
        if(myInputManager == null)
            myInputManager = FindObjectOfType<InputManager>();
        if(myAnimationSettings== null)
            myAnimationSettings = FindObjectOfType<AnimationSettings>();

        //rangedAttackQuantityOriginal = rangedAttackQuantity; // this seems wrong
        RangedAttackAnimationTime = myAnimationSettings.NinjaAnimationLengths[0]; //TODO for each enemy?
        myAnimator = GetComponent<Animator>();
        if (myAnimator == null)
        {
            Debug.Log("Error: Did not find anim!");
        }
        else
        {
            //Debug.Log("Got anim");
        }
        //UpdateAnimClipTimes();

        if (rangedAttack != null)
        {
            // Get the script type attached to the prefab
            System.Type scriptType = typeof(RangedProjectile);

            // Get the field info of the member variable you want to access
            FieldInfo fieldInfo = scriptType.GetField("moveSpeed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                // Access the member variable value from the prefab script
                object prefabScriptInstance = rangedAttack.GetComponent(scriptType);
                object fieldValue = fieldInfo.GetValue(prefabScriptInstance);
                projectileSpeed = (float)fieldValue;
                Debug.Log("Projectile speed: " + fieldValue.ToString());
            }
            else
            {
                Debug.Log("Member variable not found in the script attached to the prefab.");
            }
        }
        TimeToStart = arrivalTime;
        speed = 1 / TimeBetweenTiles;

        if (UpdateStartingPosition)
            startPos = Vector3Int.FloorToInt(transform.position);


        // Calculate the offset based on the tilemap's anchor
        offset = new Vector3(0.5f, 0.5f, 0f);

        // Initialize the grid
        // [use obstacleTilemap Instead currently.] obstacleTiles = obstacleTilemap.GetTilesBlock(obstacleTilemap.cellBounds);
    }
    


    
    //public void UpdateAnimClipTimes()
    //{
    //    AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
    //    foreach (AnimationClip clip in clips)
    //    {
           
    //        switch (clip.name)
    //        {
    //            case "Attacking":
    //                attackTime = clip.length;
    //                break;
    //            case "Damage":
    //                damageTime = clip.length;
    //                break;
    //            case "Dead":
    //                deathTime = clip.length;
    //                break;
    //            case "Idle":
    //                idleTime = clip.length;
    //                break;
    //        }
    //    }
    //}



    public void SetCurrentPath(List<PathfindingManager.Node> FoundPath)
    {
        if (FoundPath != null)
        {
            currentPath = FoundPath;
            journeyLength = FoundPath.Count;
        }
    }
    public void StartMovement()
    {
        // Find the path

        if (currentPath != null)
        {
            // Start the movement
            //TODO FIX THIS CALCULATION (Seems right, need to know timer for update.)
            TimeToStart = arrivalTime - (journeyLength / speed);
            PathfindingManager.Node targetNode = currentPath[targetIndex];
            targetPosition = obstacleTilemap.CellToWorld(targetNode.position) + offset;
        }

    }


    public void UpdatePathDistance(int Distance)
    {
        journeyLength = Distance;
    }

    public void RangedAttackAnimationComplete()
    {
        Instantiate(rangedAttack, transform.position, Quaternion.identity);
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    StartMovement();
        //}

        // Check if there is a path to follow

        
        if(Active == true)
        {
            ResolveRangeAttacks();
            ResolveVulnerableState();

            // Code below could work but seems overally complicated.//////////
            //AnimatorClipInfo[] a = myAnimator.GetCurrentAnimatorClipInfo(0);
            //string animationClipName = a[0].clip.name;
            //if(animationClipName == "VulnerableAnim")
            //{
            //    Debug.Log("Vulnerable");
            //}/////////

        }
        if (TimeToStart <= Time.timeSinceLevelLoad)
        {
            
            if (currentPath == null)
                return;
            // Calculate the endPos position based on the current time ratio
            if (targetIndex == currentPath.Count)
            {
                // Reached the destination
                targetPosition = obstacleTilemap.CellToWorld(currentPath[currentPath.Count - 1].position) + offset;
                if (transform.position == targetPosition)
                {
                    Debug.Log("Destination reached at: " + Time.timeSinceLevelLoad + transform.position);
                    currentPath = null;
                    if (UpdateStartingPosition == true)
                        startPos = Vector3Int.FloorToInt(transform.position);
                }
            }
            else
            {
                // Calculate the index of the current endPos node in the path // Travel between each tile 1 at a time, don't use timeratio
                float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
                if (distanceToTarget <= 0.01f)
                {
                    targetIndex += 1;
                    if (targetIndex != currentPath.Count)
                    {
                        PathfindingManager.Node targetNode = currentPath[targetIndex];
                        targetPosition = obstacleTilemap.CellToWorld(targetNode.position) + offset;
                    }
                }

            }

            // Move the object towards the endPos position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
    }

    public void setDead()
    {
        myAnimator.SetTrigger("Dead");
    }
    private void ResolveVulnerableState()
    {
        if (isVulnerable)
        {
            if (GlobalTimeManager.Timer > VulnerableBeat + 1 - myInputManager.inputBufferWindow && GlobalTimeManager.Timer < VulnerableBeat + 1 + myInputManager.inputBufferWindow) // within buffer window
            {
                if (myInputManager.CheckIfButtonIsPressed((int)myColour))
                {
                    if (!Dead)
                    {
                        FindObjectOfType<Player>().RangedAttack(this.gameObject);
                        Dead = true;
                        Debug.Log("Enemy: " + this.name + " Died at: " + Time.time);
                    }

                }
            }
        }
        else if (GlobalTimeManager.Timer >= VulnerableBeat)
        {
            myAnimator.SetTrigger("Vulnerable");
            // Code the part where the enemy can die.
            Invoke(methodName: "ResetVulnerableBeat", VulnerableDuration); // can cause 2 enemies to become vulnerable if 2 seconds or more..
            isVulnerable = true; //Is set back to false in LoopBeat()
            myColour = (Color)UnityEngine.Random.Range(1, 5);
        }
    }

    private void ResolveRangeAttacks()
    {
        // Create a list to store attacks to be removed
        List<RangedBeat> attacksToRemove = new List<RangedBeat>();
      
            
        for (int i = rangedAttacksList.Count - 1; i >= 0; i--)
        {
            if (rangedAttacksList[i].TimeToStart <= GlobalTimeManager.Timer)
            {
                if (rangedAttacksList[i].Done == false)
                {
                    if (MeleeMode == true)
                    {
                        myAnimator.SetTrigger("MeleeAttack");
                        rangedAttacksList[i].Done = true;
                        myColour = (Color)UnityEngine.Random.Range(1, 5);
                    }
                    else
                    {
                        myAnimator.SetTrigger("RangedAttack");
                        rangedAttacksList[i].Done = true;
                    }
                }
            }
        }

        for (int i = rangedAttacksList.Count - 1; i >= 0; i--)
        {
            if (rangedAttacksList[i].Done == true)
            {
                rangedAttacksList.RemoveAt(i);
                if (rangedAttacksList.Count == 0)
                {
                    //Play vulnerable animation here? could overlap with other attacks, is this ok? the duration could be long.
                }
            }
        }
    }

    public void MeleeAttackHit()
    {
        Player myPlayer = FindObjectOfType<Player>();
        if (myInputManager.ButtonCurrentlyPressed == (int)myColour)
        {
            myPlayer.SetReflect();
        }
        else
        {
            myPlayer.ReduceHealthBy(1);
        }    
    }


    private List<PathfindingManager.Node > AStar(PathfindingManager.Node  startNode, PathfindingManager.Node  targetNode)
    {
        List<PathfindingManager.Node > openSet = new List<PathfindingManager.Node >();
        HashSet<PathfindingManager.Node > closedSet = new HashSet<PathfindingManager.Node >();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            PathfindingManager.Node  currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (PathfindingManager.Node  neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
        
        Debug.LogWarning("No valid path found!");
        return null;
    }

    private List<PathfindingManager.Node > RetracePath(PathfindingManager.Node  startNode, PathfindingManager.Node  endNode)
    {
        List<PathfindingManager.Node > path = new List<PathfindingManager.Node >();
        PathfindingManager.Node  currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private List<PathfindingManager.Node> GetNeighbors(PathfindingManager.Node node)
    {
        List<PathfindingManager.Node> neighbors = new List<PathfindingManager.Node>();

        Vector3Int[] neighborOffsets =
        {
            new Vector3Int(-1, 0, 0),  // Left
            new Vector3Int(1, 0, 0),   // Right
            new Vector3Int(0, -1, 0),  // Down
            new Vector3Int(0, 1, 0)    // Up
        };

        foreach (var offset in neighborOffsets)
        {
            Vector3Int neighborPos = node.position + offset;

            if (nodeDictionary.TryGetValue(neighborPos, out PathfindingManager.Node neighborNode))
            {
                neighbors.Add(neighborNode);
            }
        }

        return neighbors;
    }

    private int GetDistance(PathfindingManager.Node  nodeA, PathfindingManager.Node  nodeB)
    {
        int dstX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int dstY = Mathf.Abs(nodeA.position.y - nodeB.position.y);

        return dstX + dstY;
    }

    public float SetRangedAttack(int BeatToArrive,float Distance, float BeatsPerSecond)
    {
        if(MeleeMode == true)
        {
            TimeToStart = BeatToArrive - MeleeAttackAnimationTime;
            
        }
        rangedAttackedScript = rangedAttack.GetComponent<RangedProjectile>(); // ????
        if (BeatToArrive == -1)
        {
            BeatToArrive = (int)arrivalTime;
        }
        if(Distance == 0)
        {
            //GetDistanceInt(startPos, endPos);
        }
        // forumla is Enemy Animation + travel moveSpeed / distance
        float TimeToComplete = ((Distance) / projectileSpeed) + RangedAttackAnimationTime; // Used to check if there is enough time to perform
        if (TimeToComplete + GlobalTimeManager.Timer > arrivalTime) // Since changing to myTimer from time now attacks grow in number?
        {
            Debug.Log("This Ranged Attack is unable to arrive on time.");
            return -1;
            
        }
        // Do attack
        TimeToStart = BeatToArrive - (TimeToComplete * BeatsPerSecond); // should be BeatToArrive.
        RangedBeat BeatToAdd = new RangedBeat(false, TimeToStart);
        rangedAttacksList.Add(BeatToAdd);

        return TimeToStart;

    }

    public float SetMeleeAttack(int BeatToArrive, float BeatsPerSecond)
    {
        if (BeatToArrive == -1)
        {
            BeatToArrive = (int)arrivalTime;
        }
        float TimeToComplete = MeleeAttackAnimationTime;
        TimeToStart = BeatToArrive - (TimeToComplete * BeatsPerSecond);

        RangedBeat BeatToAdd = new RangedBeat(false, TimeToStart);
        rangedAttacksList.Add(BeatToAdd);

        return TimeToStart;
    }

    public void SetVulnerable(int BeatToStart, float Duration)
    {

    }

    public void CloneAttackList()
    {
        OriginalAttackList.Clear();
        foreach(var RangedAttack in rangedAttacksList)
        {
            RangedBeat copy = new RangedBeat(RangedAttack.Done, RangedAttack.TimeToStart); // This is necessary else both references edit the same value!
            OriginalAttackList.Add(copy);
        }
        rangedAttackQuantity = OriginalAttackList.Count;
    }
    public void ResetAttackList(float Timer)
    {
        foreach (var RangedAttack in OriginalAttackList)
        {
            RangedBeat copy = new RangedBeat(RangedAttack.Done, RangedAttack.TimeToStart);
            rangedAttacksList.Add(copy);
        }
        if (rangedAttacksList.Count > 0)
        {
            Debug.Log("Enemy: " + this.name + " Has this many attacks: " + rangedAttacksList.Count + " First attack is at: " + rangedAttacksList[0].TimeToStart);
        }
       // myTimer = Timer;
    }

    public void SetTimer(float Timer)
    {
        myTimer = Timer;
    }
    public class RangedBeat
    {
        public bool Done;
        public float TimeToStart;
        public RangedBeat(bool remove, float timeToStart)
        {
            Done = remove;
            TimeToStart = timeToStart;
        }
    }


    public void CreateStringListOfActions()
    {
        StringListOfActions.Clear();
        foreach (var rangedAttack in OriginalAttackList) 
        {
            StringListOfActions.Add("Beat: " + rangedAttack.TimeToStart + " Type: " + "RangedAttack ");
        }
        if(VulnerableBeat != 999)
        {
            StringListOfActions.Add("Vulnerable Beat: " + VulnerableBeat.ToString());
        }
    }


}