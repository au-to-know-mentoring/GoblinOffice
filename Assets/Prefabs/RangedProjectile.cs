using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class RangedProjectile : MonoBehaviour
{
    public float moveSpeed = 1;
    public Transform playerTransform;
    public Color myColour;
    public SpriteRenderer spriteRenderer;
    public SettingsData GlobalSettingsObject;
    public InputManager myInputManager;
    public enum Color
    {
        Debug0,
        Green1,
        Red2,
        Blue3,
        Yellow4
    }

    // Start is called before the first frame update
    void Start()
    {
        myInputManager = FindObjectOfType<InputManager>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        myColour = (Color)Random.Range(1, 5);
        SetSpriteRendererColor();
        playerTransform = FindObjectOfType<GridMovement>().transform;
    }

    private void SetSpriteRendererColor()
    {
        switch (myColour)
        {
            case Color.Green1:
                spriteRenderer.color = GlobalSettingsObject.Green1;
                break;
            case Color.Red2:
                spriteRenderer.color = GlobalSettingsObject.Red2;
                break;
            case Color.Blue3:
                spriteRenderer.color = GlobalSettingsObject.Blue3;
                break;
            case Color.Yellow4:
                spriteRenderer.color = GlobalSettingsObject.Yellow4;
                break;
            default:
                Debug.LogWarning("Unknown color selected.");
                break;
        }
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
                Debug.Log("Button pressed is: " + myInputManager.ButtonCurrentlyPressed);
                Debug.Log("Button Needed is: " + (int)myColour);
                if (myInputManager.ButtonCurrentlyPressed == (int)myColour)
                {
                    Debug.Log("Destroyed");
                    this.gameObject.transform.position = new Vector3(5, 5, 0);
                }
            }
            }
    }

}

