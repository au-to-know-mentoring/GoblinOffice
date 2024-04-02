using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int Health;
    public int MaxHealth;
    public Animator myAnimator;
    public GameObject myProjectile;
    public GameObject myTarget;
    bool Reflect = false;
    float Counter = 0;
    public Transform ExitTransform;
    public float timeToMove;
    public bool TravelingToDoor = false;
    // Start is called before the first frame update
    void Start()
    {
        myAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Reflect) 
        {
            Counter += Time.deltaTime;
            if(Counter >= .3f)
            {
                Reflect = false;
                myAnimator.SetBool("Reflect", false);
            }

        }
        //if (TravelingToDoor)
        //{
        //    GoToExitDoor();
        //}
    }

    public void GoToExitDoor()
    {

        // Calculate the distance to move per second
        Vector3 distancePerSecond = (ExitTransform.position - transform.position) / timeToMove;

        // Calculate the distance to move per frame
        Vector3 distancePerFrame = distancePerSecond * Time.deltaTime;

        // Move the object towards the target position
        transform.Translate(distancePerFrame, Space.World);

        // Optionally, you can stop the movement when the object is close enough to the target position
        if (Vector3.Distance(transform.position, ExitTransform.position) < 0.01f)
        {
            // Snap to the target position if needed
            transform.position = ExitTransform.position;
            // Stop further movement by disabling this script or using a boolean flag
        }
    }

    public IEnumerator MoveObject(Vector3 target, float duration)
    {
        float startTime = Time.time; // Time when the movement starts
        Vector3 startPosition = transform.position; // Starting position of the object

        while (Time.time < startTime + duration)
        {
            // Calculate the fraction of the total duration that has passed
            float fractionPassed = (Time.time - startTime) / duration;
            // Update the object's position
            transform.position = Vector3.Lerp(startPosition, target, fractionPassed);
            yield return null; // Wait for the next frame
        }

        // Ensure the object is exactly at the target position at the end
        transform.position = target;
    }
    public void SetReflect()
    {
        Counter= 0;
        myAnimator.SetBool("Reflect", true);
        Reflect = true;
    }

    public void SetInjured()
    {
        Counter = 0;
        myAnimator.SetTrigger("Injured");
    }

    public void ReduceHealthBy(int damage)
    {
        Health = Health - damage;
        if(Health <= 0)
        {
            Death();
        }
        else
            SetInjured();
    }

    public void Death()
    {
        myAnimator.SetTrigger("Death");
        Health = 0;
        FindObjectOfType(typeof(PathfindingManager)).GetComponent<PathfindingManager>().LevelComplete(false);
    }
    public void RangedAttack(GameObject EnemyTarget)
    {
        myAnimator.SetTrigger("RangedAttack");
        myTarget = EnemyTarget;
    }

    public void SpawnProjectile()
    {
      GameObject instantiatedPrefab = Instantiate(myProjectile);
      PlayerRangedProjectile a = instantiatedPrefab.GetComponent<PlayerRangedProjectile>();
        a.setTarget(myTarget);
    }
}
