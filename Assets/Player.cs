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
    public Vector3 ExitPosition;
    public float timeToMove;
    public bool TravelingToDoor = false;
    [Header("Sounds")]
    public AudioSource hurtSound;
    public AudioSource attackSound;
    public AudioSource deathSound;
    public AudioSource blockSound;
    public AudioSource winSound;
    // Start is called before the first frame update
    void Start()
    {
        if (ExitTransform != null)
        {
            ExitPosition = ExitTransform.position;
        }
        else
        {
            ExitPosition = new Vector3(6.63000011f, 2.26999998f, -0.0669358075f);
        }
        myAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Reflect)
        {
            Counter += Time.deltaTime;
            if (Counter >= .3f)
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
        if (winSound != null)
            winSound.Play();
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
        Counter = 0;
        myAnimator.SetBool("Reflect", true);
        Reflect = true;
        if (blockSound != null)
            blockSound.Play();
    }

    public void SetInjured()
    {
        Counter = 0;
        myAnimator.SetTrigger("Injured");
        if (hurtSound != null)
            hurtSound.Play();
    }

    public void ReduceHealthBy(int damage)
    {
        Health = Health - damage;
        if (Health <= 0)
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
        if (deathSound != null)
            deathSound.Play();
    }
    public void RangedAttack(GameObject EnemyTarget)
    {
        myAnimator.SetTrigger("RangedAttack");
        myTarget = EnemyTarget;
        if (attackSound != null)
            attackSound.Play();
    }

    public void SpawnProjectile()
    {
        GameObject instantiatedPrefab = Instantiate(myProjectile);
        PlayerRangedProjectile a = instantiatedPrefab.GetComponent<PlayerRangedProjectile>();
        a.setTarget(myTarget);
    }
}
