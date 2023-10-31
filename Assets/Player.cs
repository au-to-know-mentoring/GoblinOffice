using System.Collections;
using System.Collections.Generic;
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
            myAnimator.SetTrigger("Death");
            Health = 0;
        }
        else
            SetInjured();
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
