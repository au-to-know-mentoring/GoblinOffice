using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerRangedProjectile : MonoBehaviour
{
    public int damage = 1;
    public float moveSpeed = 8;
    public float rotationSpeed = 360;
    public GameObject Target;
    public Transform TargetTransform;
    public Transform myChild;
    public SettingsData GlobalSettingsObject;

    private void Start()
    {
      myChild = GetComponentInChildren<Transform>();
    }
    public void setTarget(GameObject target) 
    {
        Target = target;
        TargetTransform = target.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetTransform != null)
        {
            myChild.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            // Calculate the direction from current position to the target's position
            //Change this to target
            Vector3 directionToTarget = Target.transform.position - transform.position;

            // Calculate the distance to move this frame based on moveSpeed
            float distanceToMove = moveSpeed * Time.deltaTime;

            // Limit the distance moved to not overshoot the target
            float actualDistanceToMove = Mathf.Min(distanceToMove, directionToTarget.magnitude);

            // Calculate the new position after moving towards the target
            Vector3 newPosition = transform.position + directionToTarget.normalized * actualDistanceToMove;

            // Apply the new position to the object's transform
            transform.position = newPosition;

            //Debug stuff for if hit/deflected by player:
            if (transform.position == Target.transform.position)
            {
                Target.GetComponent<PathfindingObject>().setDead();
                TargetTransform = null;
                DebugRoutine();
            }
        }
    }

    private void DebugRoutine()
    {
        if (GlobalSettingsObject.debugMode == true)
            this.gameObject.transform.position = new Vector3(-5, 6, 0);
        else
            this.gameObject.SetActive(false);
    }
}

