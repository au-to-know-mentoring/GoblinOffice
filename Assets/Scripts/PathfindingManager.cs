using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public List<PathfindingObject> pathfindingObjects = new List<PathfindingObject>();

    private void Start()
    {
        // Register all pathfinding objects in the scene
        RegisterPathfindingObjects();
    }

    private void Update()
    {
        // Update the paths for all pathfinding objects
        UpdatePaths();
    }

    private void RegisterPathfindingObjects()
    {
        // Find all pathfinding objects in the scene and add them to the list
        PathfindingObject[] objects = FindObjectsOfType<PathfindingObject>();
        pathfindingObjects.AddRange(objects);
    }

    private void UpdatePaths()
    {
        // Update the paths for each pathfinding object
        foreach (PathfindingObject pathfindingObject in pathfindingObjects)
        {
            pathfindingObject.UpdatePath();
        }
    }
}