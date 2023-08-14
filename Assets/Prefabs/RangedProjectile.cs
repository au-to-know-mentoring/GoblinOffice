using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedProjectile : MonoBehaviour
{
    public float moveSpeed = 1;
    public Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = FindObjectOfType<GridMovement>().transform;
    }

    // Update is called once per frame
    void Update()
    { 
            if (playerTransform != null)
            {
                // Calculate the direction from current position to the target's position
                Vector3 directionToTarget = playerTransform.position - transform.position;

                // Calculate the distance to move this frame based on moveSpeed
                float distanceToMove = moveSpeed * Time.deltaTime;

                // Limit the distance moved to not overshoot the target
                float actualDistanceToMove = Mathf.Min(distanceToMove, directionToTarget.magnitude);

                // Calculate the new position after moving towards the target
                Vector3 newPosition = transform.position + directionToTarget.normalized * actualDistanceToMove;

                // Apply the new position to the object's transform
                transform.position = newPosition;
            if(transform.position == playerTransform.position)
            {
                Debug.Log("Projectile: " + this.name + "Arrived at: " + Time.time);
                playerTransform = null;
            }
            }
    }

}

