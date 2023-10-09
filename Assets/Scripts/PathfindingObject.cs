using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEditor;
using System.Reflection;
using System.Collections;

public class PathfindingObject : MonoBehaviour
{
    
    public Vector3Int startPos;
    public Vector3Int endPos;
    public float TimeBetweenTiles = 1f;
    public float arrivalTime = 5f; // dont use in movement logic besides start time (DONE)
    private TileBase[] obstacleTiles;
    private Dictionary<Vector3Int, PathfindingManager.Node> nodeDictionary;
    public bool UpdateStartingPosition = true;
    [Header("Ranged Attack Variables")]
    public GameObject rangedAttack;
    public int rangedAttackQuantity;
    public int rangedAttackQuantityOriginal;
    public float delayBetweenRangedAttacks; // Change to beat manager eventually
    public float RangedAttackAnimationTime; // TODO
    private float rangedAttackDelayTimer = 0;
    private RangedProjectile rangedAttackedScript;
    public float projectileSpeed;
    //public List<string> searchTerms = new List<string> { "Ranged", "Melee", "Death" };
    public List<RangedBeat> rangedAttacksList = new List<RangedBeat>();
    public List<RangedBeat> ItemsToRemove = new List<RangedBeat>();
    public int VulnerableBeat = 999;
    public bool isVulnerable = false;
    public bool Active = false;


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

    private AnimationClip clip;

    public List<RangedBeat> OriginalAttackList = new List<RangedBeat>();

    /// </summary>
    /// <param name="obstacleTilemap"></param>
    public void setObstacleTilemap(Tilemap obstacleTilemap)
    {
        this.obstacleTilemap = obstacleTilemap;

    }

    private void Start()
    {
        rangedAttackQuantityOriginal = rangedAttackQuantity;
        RangedAttackAnimationTime = myAnimationSettings.NinjaAnimationLengths[0];
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
                Debug.Log("Member variable value: " + fieldValue.ToString());
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
        // Get the obstacle tiles from the tilemap
        obstacleTiles = obstacleTilemap.GetTilesBlock(obstacleTilemap.cellBounds);

        // Calculate the offset based on the tilemap's anchor
        offset = new Vector3(0.5f, 0.5f, 0f);

        // Initialize the grid

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

        myTimer += Time.deltaTime;
        if(Active == true)
        {
            ResolveRangeAttacks();
            if(myTimer >= VulnerableBeat)
            {
                myAnimator.SetTrigger("Vulnerable");
                // Code the part where the enemy can die.
                VulnerableBeat = 999;
                isVulnerable= true; //Is set back to false in LoopBeat()
            }

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

    private void ResolveRangeAttacks()
    {
        // Create a list to store attacks to be removed
        List<RangedBeat> attacksToRemove = new List<RangedBeat>();

        for (int i = rangedAttacksList.Count - 1; i >= 0; i--)
        {
            if (rangedAttacksList[i].TimeToStart <= myTimer)
            {
                if (rangedAttacksList[i].Done == false)
                {
                    myAnimator.SetTrigger("RangedAttack");
                    rangedAttacksList[i].Done = true;
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

    public float SetRangedAttack(int BeatToArrive,float Distance)
    {
        rangedAttackedScript = rangedAttack.GetComponent<RangedProjectile>();
        if (BeatToArrive == -1)
        {
            BeatToArrive = (int)arrivalTime;
        }
        if(Distance == 0)
        {
            //GetDistance(startPos, endPos);
        }
        // forumla is Enemy Animation + travel moveSpeed / distance
        float TimeToComplete = ((Distance) / projectileSpeed) + RangedAttackAnimationTime; // Used to check if there is enough time to perform
        if (TimeToComplete + Time.time > arrivalTime)
        {
            Debug.Log("This Ranged Attack is unable to arrive on time.");
            return -1;
            
        }
        // Do attack
        TimeToStart = BeatToArrive - TimeToComplete; // should be BeatToArrive.
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
        rangedAttackQuantity = rangedAttackQuantityOriginal;
    }
    public void ResetAttackList(float Timer)
    {
        foreach (var RangedAttack in OriginalAttackList)
        {
            RangedBeat copy = new RangedBeat(RangedAttack.Done, RangedAttack.TimeToStart);
            rangedAttacksList.Add(copy);
        }
        Debug.Log("Enemy: " + this.name + " Has this many attacks: " + rangedAttacksList.Count + " First attack is at: " + rangedAttacksList[0].TimeToStart);
        
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


}