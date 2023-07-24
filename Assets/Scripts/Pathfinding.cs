//using UnityEngine;
//using UnityEngine.Tilemaps;
//using System.Collections.Generic;

//public class Pathfinding : MonoBehaviour
//{
//    public Tilemap obstacleTilemap;
//    public Vector3Int startPos;
//    public Vector3Int endPos;
//    public float speed = 5f;
//    public float arrivalTime = 5f;

//    private TileBase[] obstacleTiles;
//    private Dictionary<Vector3Int, Node> nodeDictionary = new Dictionary<Vector3Int, Node>();

//    private List<Node> currentPath;
//    private float startTime;
//    private float journeyLength;

//    private Vector3 offset;

//    private void Start()
//    {
//        // Get the obstacle tiles from the tilemap
//        obstacleTiles = obstacleTilemap.GetTilesBlock(obstacleTilemap.cellBounds);

//        // Calculate the offset based on the tilemap's anchor
//        offset = new Vector3(0.5f, 0.5f, 0f);

//        // Initialize the grid
//        InitializeGrid();
//    }

//    private void StartMovement()
//    {
//        // Find the path
//        currentPath = FindPath(startPos, endPos);
//        if (currentPath != null)
//        {
//            // Start the movement
//            startTime = Time.time;
//            journeyLength = Vector3.Distance(startPos, endPos);
//        }
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            StartMovement();
//        }

//        // Check if there is a path to follow
//        if (currentPath == null)
//            return;

//        // Calculate the current time ratio based on the arrival time
//        float timeRatio = (Time.time - startTime) / arrivalTime;

//        // Calculate the endPos position based on the current time ratio
//        Vector3 targetPosition;
//        if (timeRatio >= 1f)
//        {
//            // Reached the destination
//            Debug.Log("Desitination reached " + transform.position);
//            targetPosition = obstacleTilemap.CellToWorld(currentPath[currentPath.Count - 1].position) + offset;
//            Debug.Log("Target Position is: " + targetPosition.y);
//            if (transform.position == targetPosition)
//            {
//                currentPath = null;
//            }
//        }
//        else
//        {
//            // Calculate the index of the current endPos node in the path
//            int targetIndex = Mathf.Clamp(Mathf.FloorToInt(timeRatio * (currentPath.Count - 1)), 0, currentPath.Count - 1);
//            Node targetNode = currentPath[targetIndex];
//            targetPosition = obstacleTilemap.CellToWorld(targetNode.position) + offset;
//        }

//        // Move the object towards the endPos position
//        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
//    }



//    private void InitializeGrid()
//    {
//        // Loop through each cell in the tilemap
//        foreach (var position in obstacleTilemap.cellBounds.allPositionsWithin)
//        {
//            Vector3Int cellPosition = new Vector3Int(position.x, position.y, position.z);

//            // Create a new node for the cell
//            Node node = new Node(cellPosition);

//            // Check if the cell contains an obstacle tile
//            if (obstacleTiles != null && obstacleTilemap.HasTile(cellPosition))
//            {
//                node.isWalkable = false;
//            }

//            // Add the node to the dictionary
//            nodeDictionary.Add(cellPosition, node);
//        }
//    }

//    private void DrawDebugLines()
//    {
//        // Draw debug lines for the grid
//        foreach (var node in nodeDictionary.Values)
//        {
//            Vector3Int cellPosition = node.position;
//            Vector3 worldPosition = obstacleTilemap.CellToWorld(cellPosition);
//            Vector3 cellSize = obstacleTilemap.cellSize;
//            Vector3 nodeCenter = worldPosition + new Vector3(cellSize.x / 2f, cellSize.y / 2f, 0f);
//            float radius = Mathf.Min(cellSize.x, cellSize.y) * 0.1f;

//            Debug.DrawRay(nodeCenter, Vector3.forward, Color.yellow, 9999f);
//            Debug.DrawRay(nodeCenter, Vector3.up * radius, Color.yellow, 9999f);
//            Debug.DrawRay(nodeCenter, Vector3.down * radius, Color.yellow, 9999f);
//            Debug.DrawRay(nodeCenter, Vector3.left * radius, Color.yellow, 9999f);
//            Debug.DrawRay(nodeCenter, Vector3.right * radius, Color.yellow, 9999f);



//            Vector3 topLeft = worldPosition + new Vector3(-cellSize.x / 2f + .5f, -cellSize.y / 2f + .5f);
//            Vector3 topRight = worldPosition + new Vector3(cellSize.x / 2f + .5f, -cellSize.y / 2f + .5f);
//            Vector3 bottomLeft = worldPosition + new Vector3(-cellSize.x / 2f + .5f, cellSize.y / 2f + .5f);
//            Vector3 bottomRight = worldPosition + new Vector3(cellSize.x / 2f + .5f, cellSize.y / 2f + .5f);

//            Debug.DrawLine(topLeft, topRight, Color.green, 100000f);
//            Debug.DrawLine(topRight, bottomRight, Color.green, 100000f);
//            Debug.DrawLine(bottomRight, bottomLeft, Color.green, 100000f);
//            Debug.DrawLine(bottomLeft, topLeft, Color.green, 100000f);
//        }

//        // Draw debug lines for obstacles
//        foreach (var position in obstacleTilemap.cellBounds.allPositionsWithin)
//        {
//            Vector3Int cellPosition = new Vector3Int(position.x, position.y, position.z);

//            if (obstacleTilemap.HasTile(cellPosition))
//            {
//                Vector3 worldPosition = obstacleTilemap.CellToWorld(cellPosition);
//                Vector3 cellSize = obstacleTilemap.cellSize;

//                Vector3 topLeft = worldPosition + new Vector3(-cellSize.x / 2f, -cellSize.y / 2f);
//                Vector3 topRight = worldPosition + new Vector3(cellSize.x / 2f, -cellSize.y / 2f);
//                Vector3 bottomLeft = worldPosition + new Vector3(-cellSize.x / 2f, cellSize.y / 2f);
//                Vector3 bottomRight = worldPosition + new Vector3(cellSize.x / 2f, cellSize.y / 2f);

//                Debug.DrawLine(topLeft, topRight, Color.red, 10000f);
//                Debug.DrawLine(topRight, bottomRight, Color.red, 10000f);
//                Debug.DrawLine(bottomRight, bottomLeft, Color.red, 10000f);
//                Debug.DrawLine(bottomLeft, topLeft, Color.red, 10000f);
//            }
//        }
//    }

//    private List<Node> FindPath(Vector3 startPos, Vector3 endPos)
//    {
//        Vector3Int startCell = obstacleTilemap.WorldToCell(startPos);
//        Vector3Int targetCell = obstacleTilemap.WorldToCell(endPos);

//        if (!nodeDictionary.ContainsKey(startCell) || !nodeDictionary.ContainsKey(targetCell))
//        {
//            Debug.LogWarning("Invalid startPos or endPos position!");
//            return null;
//        }

//        Node startNode = nodeDictionary[startCell];
//        Node targetNode = nodeDictionary[targetCell];

//        // Run the A* algorithm to find the path
//        List<Node> path = AStar(startNode, targetNode);

//        // Draw the debug line
//        if (path != null)
//        {
//            for (int i = 0; i < path.Count - 1; i++)
//            {
//                Vector3 startPoint = obstacleTilemap.CellToWorld(path[i].position) + obstacleTilemap.cellSize * 0.5f;
//                Vector3 endPoint = obstacleTilemap.CellToWorld(path[i + 1].position) + obstacleTilemap.cellSize * 0.5f;
//                Debug.DrawLine(startPoint, endPoint, Color.blue, 10000f);
//                path[i].isWalkable = false;
                
//            }
//        }

//        return path;
//    }

//    private List<Node> AStar(Node startNode, Node targetNode)
//    {
//        List<Node> openSet = new List<Node>();
//        HashSet<Node> closedSet = new HashSet<Node>();

//        openSet.Add(startNode);

//        while (openSet.Count > 0)
//        {
//            Node currentNode = openSet[0];

//            for (int i = 1; i < openSet.Count; i++)
//            {
//                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
//                {
//                    currentNode = openSet[i];
//                }
//            }

//            openSet.Remove(currentNode);
//            closedSet.Add(currentNode);

//            if (currentNode == targetNode)
//            {
//                return RetracePath(startNode, targetNode);
//            }

//            foreach (Node neighbor in GetNeighbors(currentNode))
//            {
//                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
//                {
//                    continue;
//                }

//                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

//                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
//                {
//                    neighbor.gCost = newMovementCostToNeighbor;
//                    neighbor.hCost = GetDistance(neighbor, targetNode);
//                    neighbor.parent = currentNode;

//                    if (!openSet.Contains(neighbor))
//                    {
//                        openSet.Add(neighbor);
//                    }
//                }
//            }
//        }

//        Debug.LogWarning("No valid path found!");
//        return null;
//    }

//    private List<Node> RetracePath(Node startNode, Node endNode)
//    {
//        List<Node> path = new List<Node>();
//        Node currentNode = endNode;

//        while (currentNode != startNode)
//        {
//            path.Add(currentNode);
//            currentNode = currentNode.parent;
//        }

//        path.Reverse();
//        return path;
//    }

//    private List<Node> GetNeighbors(Node node)
//    {
//        List<Node> neighbors = new List<Node>();

//        Vector3Int[] neighborOffsets =
//        {
//            new Vector3Int(-1, 0, 0),  // Left
//            new Vector3Int(1, 0, 0),   // Right
//            new Vector3Int(0, -1, 0),  // Down
//            new Vector3Int(0, 1, 0)    // Up
//        };

//        foreach (var offset in neighborOffsets)
//        {
//            Vector3Int neighborPos = node.position + offset;

//            if (nodeDictionary.TryGetValue(neighborPos, out Node neighborNode))
//            {
//                neighbors.Add(neighborNode);
//            }
//        }

//        return neighbors;
//    }

//    private int GetDistance(Node nodeA, Node nodeB)
//    {
//        int dstX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
//        int dstY = Mathf.Abs(nodeA.position.y - nodeB.position.y);

//        return dstX + dstY;
//    }

//    private class Node
//    {
//        public Vector3Int position;
//        public bool isWalkable;
//        public int gCost;
//        public int hCost;
//        public Node parent;

//        public int fCost => gCost + hCost;

//        public Node(Vector3Int pos)
//        {
//            position = pos;
//            isWalkable = true;
//            gCost = int.MaxValue;
//            hCost = 0;
//            parent = null;
//        }
//    }
//}