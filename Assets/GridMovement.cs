using UnityEngine;
using UnityEngine.Tilemaps;

public class GridMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    public Tilemap tilemap; // Reference to the Tilemap component
    public TileBase obstacleTile; // Tile representing an obstacle
    private Vector3 targetPosition; // Target world position for movement
    private bool isMoving; // Flag to indicate if the object is currently moving

    // Add any additional variables you need for your game logic

    private void Update()
    {
        if (isMoving)
        {
            // Move towards the targetPos position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * moveSpeed);

            // Check if the object has reached the targetPos position
            if (transform.position == targetPosition)
            {
                isMoving = false;
            }
        }
        else
        {
            // Check for input to initiate movement
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            // Calculate the next grid position based on the input
            Vector3Int nextCell = tilemap.WorldToCell(transform.position) + new Vector3Int(Mathf.RoundToInt(horizontalInput), Mathf.RoundToInt(verticalInput), 0);

            // Check if the next cell is valid (e.g., not an obstacle)
            if (IsCellValid(nextCell))
            {
                // Calculate the targetPos position in world coordinates
                targetPosition = tilemap.GetCellCenterWorld(nextCell);

                // Start moving towards the targetPos position
                isMoving = true;
            }
        }
    }

    private bool IsCellValid(Vector3Int cell)
    {
        // Check if the tile at the given cell is an obstacle tile
        return !tilemap.HasTile(cell) || tilemap.GetTile(cell) != obstacleTile;
    }
}