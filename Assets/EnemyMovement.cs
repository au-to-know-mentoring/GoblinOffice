//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class EnemyMovement : MonoBehaviour
//{
//    public float moveSpeed = 1f; // Speed of movement
//    public Tilemap tilemap; // Reference to the Tilemap component
//    public Transform player; // Reference to the player's Transform component
//    private Vector3 targetPosition; // Target world position for movement
//    private bool isMoving; // Flag to indicate if the enemy is currently moving

//    private void Start()
//    {
//        // Start the initial movement towards the player
//        MoveTowardsPlayer();
//    }

//    private void Update()
//    {
//        if (isMoving)
//        {
//            // Move towards the target position
//            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * moveSpeed);

//            // Check if the enemy has reached the target position
//            if (transform.position == targetPosition)
//            {
//                isMoving = false;

//                // Start the next movement towards the player
//                MoveTowardsPlayer();
//            }
//        }
//    }

//    private void MoveTowardsPlayer()
//    {
//        // Calculate the current cell position of the enemy
//        Vector3Int currentCell = tilemap.WorldToCell(transform.position);

//        // Calculate the target cell position towards the player
//        Vector3Int targetCell = tilemap.WorldToCell(player.position);

//        // Calculate the path between the current cell and the target cell
//        Vector3Int[] path = tilemap.GetPath(currentCell, targetCell);

//        // Check if a valid path exists
//        if (path != null && path.Length > 0)
//        {
//            // Set the target position to the center of the next cell in the path
//            targetPosition = tilemap.GetCellCenterWorld(path[0]);

//            // Start moving towards the target position
//            isMoving = true;
//        }
//    }
//}