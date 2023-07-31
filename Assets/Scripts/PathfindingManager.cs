using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
public class PathfindingManager : MonoBehaviour
{
    public List<PathfindingObject> pathfindingObjects = new List<PathfindingObject>();
    public GameObject Player;
    public Tilemap obstacleTilemap;
    private TileBase[] obstacleTiles;
    private Vector3 offset;
    public Dictionary<Vector3Int, Node> nodeDictionary = new Dictionary<Vector3Int, Node>();
    
    public float myTimer;
    
    public Vector3Int LeftOfPlayerPosition;
    public Vector3Int RightOfPlayerPosition;
    public Vector3Int TopOfPlayerPosition;
    private void Start()
    {
        
        // Set Positions adjacent to player.
        SetPositionsAroundPlayer();
        // Register all pathfinding objects in the scene
        RegisterPathfindingObjects();
        
        // Get the obstacle tiles from the tilemap
        obstacleTiles = obstacleTilemap.GetTilesBlock(obstacleTilemap.cellBounds);

        // Calculate the offset based on the tilemap's anchor
        offset = new Vector3(0.5f, 0.5f, 0f);

        // Initialize the grid
        InitializeGrid();
        DrawDebugLines();
    }


        

    public Dictionary<Vector3Int, Node> GetNodeDictionary()
    {
        return nodeDictionary;
    }
    private void Update()
    {
        myTimer += Time.deltaTime;
        // Set Paths to surround Player.
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q is pressed");
            AssignPathsToSurroundPlayer();
        }

        // Update the paths for all pathfinding objects
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach(var pathFindingObject in pathfindingObjects) 
            {
               pathFindingObject.SetCurrentPath(FindPath(pathFindingObject.startPos, pathFindingObject.endPos));
               pathFindingObject.StartMovement();
            }
        }
        //UpdatePaths();
    }

    private void AssignPathsToSurroundPlayer()
    {
        //**[Optimize] Could be optimized by placing all pathFindingObjects into a list, and taking objects out after assigning them.
        //**[Improvement] Current priority order is (Left, Right, Top). Could be changed to help equal out the arrival times.
        //**[Limitations] Cannot Assign more then 3 enemies atm. Cannot Assign more then 1 enemy to 1 spot.

        PathfindingObject EnemyForLeftPosition = null;
        PathfindingObject EnemyForRightPosition = null;
        PathfindingObject EnemyForTopPosition = null;

        int DistanceToLeftPos = 999;
        int DistanceToRightPos = 999;
        int DistanceToTopPos = 999;
        //LEFT
        foreach (var pathFindingObject in pathfindingObjects)
        {
            Debug.Log(GetDistance(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(LeftOfPlayerPosition)]));

            if (GetDistance(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(LeftOfPlayerPosition)]) < DistanceToLeftPos)
            {
                EnemyForLeftPosition = pathFindingObject;
                DistanceToLeftPos = GetDistance(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(LeftOfPlayerPosition)]);
            }
        }
        Debug.Log("The closest Enemy to the position left of the player is: " + EnemyForLeftPosition.name);
        EnemyForLeftPosition.endPos = Vector3Int.FloorToInt(LeftOfPlayerPosition);
        //RIGHT
        foreach (var pathFindingObject in pathfindingObjects)
        {
            Debug.Log(GetDistance(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(RightOfPlayerPosition)]));

            if (GetDistance(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(RightOfPlayerPosition)]) < DistanceToRightPos)
            {
                if (pathFindingObject != EnemyForLeftPosition)
                {
                    EnemyForRightPosition = pathFindingObject;
                    DistanceToRightPos = GetDistance(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(RightOfPlayerPosition)]);
                }
            }
        }
        Debug.Log("The closest Enemy to the position Right of the player is: " + EnemyForRightPosition.name);
        EnemyForLeftPosition.endPos = Vector3Int.FloorToInt(RightOfPlayerPosition);
        //TOP
        foreach (var pathFindingObject in pathfindingObjects)
        {
            Debug.Log(GetDistance(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(TopOfPlayerPosition)]));

            if (GetDistance(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(TopOfPlayerPosition)]) < DistanceToTopPos)
            {
                if (pathFindingObject != EnemyForLeftPosition & pathFindingObject != EnemyForRightPosition)
                {
                    EnemyForTopPosition = pathFindingObject;
                    DistanceToTopPos = GetDistance(nodeDictionary[pathFindingObject.startPos], nodeDictionary[Vector3Int.FloorToInt(TopOfPlayerPosition)]);
                }
            }
        }
        Debug.Log("The closest Enemy to the position Top of the player is: " + EnemyForTopPosition.name);
        EnemyForLeftPosition.endPos = LeftOfPlayerPosition;
        if (EnemyForRightPosition != null)
        {
            EnemyForRightPosition.endPos = RightOfPlayerPosition;
            if (EnemyForTopPosition != null)
            {
                EnemyForTopPosition.endPos = TopOfPlayerPosition;
            }
        }
    }

    private void RegisterPathfindingObjects()
    {
        // Find all pathfinding objects in the scene and add them to the list
        PathfindingObject[] objects = FindObjectsOfType<PathfindingObject>();
        pathfindingObjects.AddRange(objects);
        foreach (var pathFindingObject in pathfindingObjects)
        {
            pathFindingObject.setObstacleTilemap(obstacleTilemap);
        }
    }
    private void SetPositionsAroundPlayer()
    {
        LeftOfPlayerPosition = Vector3Int.FloorToInt(Player.transform.position + Vector3.left);
        RightOfPlayerPosition = Vector3Int.FloorToInt(Player.transform.position + Vector3.right);
        TopOfPlayerPosition = Vector3Int.FloorToInt(Player.transform.position + Vector3.up);
    }


    //private void UpdatePaths()
    //{
    //    // Update the paths for each pathfinding object
    //    foreach (PathfindingObject pathfindingObject in pathfindingObjects)
    //    {
    //        pathfindingObject.UpdatePath();
    //    }
    //}
    private void InitializeGrid()
    {
        // Loop through each cell in the tilemap
        foreach (var position in obstacleTilemap.cellBounds.allPositionsWithin)
        {
            Vector3Int cellPosition = new Vector3Int(position.x, position.y, position.z);

            // Create a new node for the cell
            Node node = new Node(cellPosition);

            // Check if the cell contains an obstacle tile
            if (obstacleTiles != null && obstacleTilemap.HasTile(cellPosition))
            {
                node.isWalkable = false;
            }

            // Add the node to the dictionary
            nodeDictionary.Add(cellPosition, node);
        }
    }
    private void DrawDebugLines()
    {
        // Draw debug lines for the grid
        foreach (var node in nodeDictionary.Values)
        {
            Vector3Int cellPosition = node.position;
            Vector3 worldPosition = obstacleTilemap.CellToWorld(cellPosition);
            Vector3 cellSize = obstacleTilemap.cellSize;
            Vector3 nodeCenter = worldPosition + new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0f);
            float radius = Mathf.Min(cellSize.x, cellSize.y) * 0.1f;

            Debug.DrawRay(nodeCenter, Vector3.forward, Color.yellow, 9999f);
            Debug.DrawRay(nodeCenter, Vector3.up * radius, Color.yellow, 9999f);
            Debug.DrawRay(nodeCenter, Vector3.down * radius, Color.yellow, 9999f);
            Debug.DrawRay(nodeCenter, Vector3.left * radius, Color.yellow, 9999f);
            Debug.DrawRay(nodeCenter, Vector3.right * radius, Color.yellow, 9999f);



            Vector3 topLeft = worldPosition + new Vector3(-cellSize.x / 2f + .5f, -cellSize.y / 2f + .5f);
            Vector3 topRight = worldPosition + new Vector3(cellSize.x / 2f + .5f, -cellSize.y / 2f + .5f);
            Vector3 bottomLeft = worldPosition + new Vector3(-cellSize.x / 2f + .5f, cellSize.y / 2f + .5f);
            Vector3 bottomRight = worldPosition + new Vector3(cellSize.x / 2f + .5f, cellSize.y / 2f + .5f);

            Debug.DrawLine(topLeft, topRight, Color.green, 100000f);
            Debug.DrawLine(topRight, bottomRight, Color.green, 100000f);
            Debug.DrawLine(bottomRight, bottomLeft, Color.green, 100000f);
            Debug.DrawLine(bottomLeft, topLeft, Color.green, 100000f);
        }

        // Draw debug lines for obstacles
        foreach (var position in obstacleTilemap.cellBounds.allPositionsWithin)
        {
            Vector3Int cellPosition = new Vector3Int(position.x, position.y, position.z);

            if (obstacleTilemap.HasTile(cellPosition))
            {
                Vector3 worldPosition = obstacleTilemap.CellToWorld(cellPosition);
                Vector3 cellSize = obstacleTilemap.cellSize;

                Vector3 topLeft = worldPosition + new Vector3(-cellSize.x / 2f, -cellSize.y / 2f);
                Vector3 topRight = worldPosition + new Vector3(cellSize.x / 2f, -cellSize.y / 2f);
                Vector3 bottomLeft = worldPosition + new Vector3(-cellSize.x / 2f, cellSize.y / 2f);
                Vector3 bottomRight = worldPosition + new Vector3(cellSize.x / 2f, cellSize.y / 2f);

                Debug.DrawLine(topLeft, topRight, Color.red, 10000f);
                Debug.DrawLine(topRight, bottomRight, Color.red, 10000f);
                Debug.DrawLine(bottomRight, bottomLeft, Color.red, 10000f);
                Debug.DrawLine(bottomLeft, topLeft, Color.red, 10000f);
            }
        }
    }

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3Int startCell = obstacleTilemap.WorldToCell(startPos);
        Vector3Int targetCell = obstacleTilemap.WorldToCell(targetPos);
        if(startCell == targetCell)
        {
            Debug.Log("Can't find path when Start and target cell are the same Object: " + this.name);
            return null;
        }    
        if (!nodeDictionary.ContainsKey(startCell) || !nodeDictionary.ContainsKey(targetCell))
        {
            Debug.LogWarning("Invalid start or target position!");
            return null;
        }

        Node startNode = nodeDictionary[startCell];
        Node targetNode = nodeDictionary[targetCell];

        // Run the A* algorithm to find the path
        List<Node> path = AStar(startNode, targetNode);
        
        // Draw the debug line
        if (path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 startPoint = obstacleTilemap.CellToWorld(path[i].position) + obstacleTilemap.cellSize * 0.5f;
                Vector3 endPoint = obstacleTilemap.CellToWorld(path[i + 1].position) + obstacleTilemap.cellSize * 0.5f;
                Debug.DrawLine(startPoint, endPoint, Color.blue, 10000f);
                //Disable Path?
                //path[i].isWalkable = false;

            }
        }
        
        return path;
    }

    private List<Node> AStar(Node startNode, Node targetNode)
    {
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];

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

            foreach (Node neighbor in GetNeighbors(currentNode))
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

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

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

            if (nodeDictionary.TryGetValue(neighborPos, out Node neighborNode))
            {
                neighbors.Add(neighborNode);
            }
        }

        return neighbors;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int dstY = Mathf.Abs(nodeA.position.y - nodeB.position.y);

        return dstX + dstY;
    }

    public class Node
    {
        public Vector3Int position;
        public bool isWalkable;
        public int gCost;
        public int hCost;
        public Node parent;

        public int fCost => gCost + hCost;

        public Node(Vector3Int pos)
        {
            position = pos;
            isWalkable = true;
            gCost = int.MaxValue;
            hCost = 0;
            parent = null;
        }
    }
}