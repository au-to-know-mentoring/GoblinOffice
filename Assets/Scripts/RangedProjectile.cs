using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static SharedEnums;  // Add this line to use the shared enum

public class RangedProjectile : MonoBehaviour
{
    public int damage = 1;
    public float moveSpeed = 1;
    public Player myPlayer;
    public Transform playerTransform;
    public ColorType myColour;  // Change this line
    public SpriteRenderer spriteRenderer;
    public SettingsData GlobalSettingsObject;
    public InputManager myInputManager;
    public bool SpeedAdjusted = false;

    // Start is called before the first frame update
    void Start()
    {
        
        //moveSpeed *= GlobalSettingsObject.BeatsPerSecondBPM;
        myInputManager = FindObjectOfType<InputManager>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        //myColour = (Color)Random.Range(1, 5);
        SetSpriteRendererColor();
        myPlayer = FindObjectOfType<Player>();
        playerTransform = myPlayer.GetComponent<Transform>();
        Vector3 lookDirection;
        lookDirection = myPlayer.transform.position - transform.position;
    

    // Calculate the angle in degrees
    float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        angle += 90f;
        // Rotate the object
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void SetColor(ColorType color)  // Update this method
    {
        myColour = color;
        SetSpriteRendererColor();
    }
    private void SetSpriteRendererColor()
    {
        if(spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        switch (myColour)
        {
            case ColorType.Green1:
                spriteRenderer.color = GlobalSettingsObject.Green1;
                break;
            case ColorType.Red2:
                spriteRenderer.color = GlobalSettingsObject.Red2;
                break;
            case ColorType.Blue3:
                spriteRenderer.color = GlobalSettingsObject.Blue3;
                break;
            case ColorType.Yellow4:
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
            
            //Debug stuff for if hit/deflected by player:
            if(transform.position == playerTransform.position)
            {
                //Debug.Log("Projectile: " + this.name + "Arrived at: " + Time.time);
                playerTransform = null;
                //Debug.Log("Button pressed is: " + myInputManager.ButtonCurrentlyPressed);
                //Debug.Log("Button Needed is: " + (int)myColour);
                if (myInputManager.ButtonCurrentlyPressed == (int)myColour)
                {
                    Debug.Log("Destroyed");
                    myPlayer.SetReflect();
                    if (GlobalSettingsObject.debugMode == true)
                    this.gameObject.transform.position = new Vector3(5, 5, 0);
                    else
                        this.gameObject.SetActive(false);

                }
                else
                {
                    myPlayer.ReduceHealthBy(damage);
                    if (GlobalSettingsObject.debugMode == true)
                        this.gameObject.transform.position = new Vector3(-5, 5, 0);
                    else
                        this.gameObject.SetActive(false);
                }
            }
            }
    }




}
