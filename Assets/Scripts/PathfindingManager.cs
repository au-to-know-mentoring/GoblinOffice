//using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
//using UnityEngine;
//using UnityEngine.Tilemaps;
//public class PathfindingManager : MonoBehaviour
//{
//    public List<PathfindingObject> pathfindingObjects = new List<PathfindingObject>();
//    public Tilemap obstacleTilemap;
//    private TileBase[] obstacleTiles;
//    private Vector3 offset;
//    private void Start()
//    {
//        // Register all pathfinding objects in the scene
//        RegisterPathfindingObjects();
//        // Get the obstacle tiles from the tilemap
//        obstacleTiles = obstacleTilemap.GetTilesBlock(obstacleTilemap.cellBounds);

//        // Calculate the offset based on the tilemap's anchor
//        offset = new Vector3(0.5f, 0.5f, 0f);

//        // Initialize the grid
//        InitializeGrid();
//    }

//    private void Update()
//    {
//        // Update the paths for all pathfinding objects
//        UpdatePaths();
//    }

//    private void RegisterPathfindingObjects()
//    {
//        // Find all pathfinding objects in the scene and add them to the list
//        PathfindingObject[] objects = FindObjectsOfType<PathfindingObject>();
//        pathfindingObjects.AddRange(objects);
//    }

//    private void UpdatePaths()
//    {
//        // Update the paths for each pathfinding object
//        foreach (PathfindingObject pathfindingObject in pathfindingObjects)
//        {
//            pathfindingObject.UpdatePath();
//        }
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
//}