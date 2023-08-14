using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEditor;
using System.Reflection;

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
    public float delayBetweenRangedAttacks; // Change to beat manager eventually
    public float RangedAttackAnimationTime; // TODO
    private float rangedAttackDelayTimer = 0;
    private RangedProjectile rangedAttackedScript;
    public float projectileSpeed;

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
    private Animator myAnimator;

    private bool Assigned;
    /// </summary>
    /// <param name="obstacleTilemap"></param>
    public void setObstacleTilemap(Tilemap obstacleTilemap)
    {
        this.obstacleTilemap = obstacleTilemap;
    }

    private void Start()
    {
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
        myAnimator = GetComponent<Animator>();
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

        

        if (TimeToStart <= Time.timeSinceLevelLoad)
        {

            if (rangedAttackQuantity >= 1) 
            {
                rangedAttackQuantity--;
                if(rangedAttackDelayTimer <= 0) // Replace with beat events.
                {
                    myAnimator.SetTrigger("RangedAttack");
                    
                    
                }

            }

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
                if(distanceToTarget <= 0.01f)
                {
                    targetIndex += 1;
                    if(targetIndex != currentPath.Count)
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

    public bool SetRangedAttack(int BeatToArrive,float Distance)
    {
        rangedAttackedScript = rangedAttack.GetComponent<RangedProjectile>();
        if (BeatToArrive == 0)
        {
            BeatToArrive = (int)arrivalTime;
        }
        if(Distance == 0)
        {
            //GetDistance(startPos, endPos);
        }
        // forumla is Enemy Animation + travel moveSpeed / distance
        float TimeToComplete = ((Distance + 1) / projectileSpeed) + RangedAttackAnimationTime; // Used to check if there is enough time to perform
        if (TimeToComplete + Time.time > arrivalTime)
        {
            Debug.Log("This Ranged Attack is unable to arrive on time.");
            return false;
            
        }
        // Do attack
        TimeToStart = arrivalTime - TimeToComplete;
        

        return true;

    }

}