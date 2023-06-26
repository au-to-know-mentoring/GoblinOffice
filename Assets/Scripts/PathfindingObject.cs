using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PathfindingObject : MonoBehaviour
{
    public Tilemap obstacleTilemap;
    public Vector3Int startPos;
    public Vector3Int targetPos;
    public float speed = 5f;
    public float arrivalTime = 5f;
    public PathfindingManager pathfindingManager;
    private TileBase[] obstacleTiles;
    private Dictionary<Vector3Int, PathfindingManager.Node> nodeDictionary;

    private List<PathfindingManager.Node> currentPath;
    private float startTime;
    private float journeyLength;

    private Vector3 offset;

    private void Start()
    {
        // Get the obstacle tiles from the tilemap
        obstacleTiles = obstacleTilemap.GetTilesBlock(obstacleTilemap.cellBounds);

        // Calculate the offset based on the tilemap's anchor
        offset = new Vector3(0.5f, 0.5f, 0f);

        // Initialize the grid

    }
    public void SetCurrentPath(List<PathfindingManager.Node> FoundPath)
    {
        currentPath = FoundPath;
    }
    public void StartMovement()
    {
        // Find the path

        if (currentPath != null)
        {
            // Start the movement
            startTime = Time.time;
            journeyLength = Vector3.Distance(startPos, targetPos);
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    StartMovement();
        //}

        // Check if there is a path to follow
        if (currentPath == null)
            return;

        // Calculate the current time ratio based on the arrival time
        float timeRatio = (Time.time - startTime) / arrivalTime;

        // Calculate the targetPos position based on the current time ratio
        Vector3 targetPosition;
        if (timeRatio >= 1f)
        {
            // Reached the destination
            Debug.Log("Desitination reached " + transform.position);
            targetPosition = pathfindingManager.obstacleTilemap.CellToWorld(currentPath[currentPath.Count - 1].position) + offset;
            Debug.Log("Target Position is: " + targetPosition.y);
            if (transform.position == targetPosition)
            {
                currentPath = null;
            }
        }
        else
        {
            // Calculate the index of the current targetPos node in the path
            int targetIndex = Mathf.Clamp(Mathf.FloorToInt(timeRatio * (currentPath.Count - 1)), 0, currentPath.Count - 1);
            PathfindingManager.Node targetNode = currentPath[targetIndex];
            targetPosition = obstacleTilemap.CellToWorld(targetNode.position) + offset;
        }

        // Move the object towards the targetPos position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
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

    private List<PathfindingManager.Node > GetNeighbors(PathfindingManager.Node  node)
    {
        List<PathfindingManager.Node > neighbors = new List<PathfindingManager.Node >();

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

            if (nodeDictionary.TryGetValue(neighborPos, out PathfindingManager.Node  neighborNode))
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

}