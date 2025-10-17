using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShrimpTest : MonoBehaviour
{
    [SerializeField] float minX = -5f, maxX = 5f;
    [SerializeField] float minY = -3f, maxY = 3f;

    public Transform shrimpvisual;
  

    [SerializeField]
    Transform[] possibleTargets;

    [SerializeField]
    float lerpTimeMax;

    [SerializeField]
    AnimationCurve idleWalkCurve;

    [SerializeField]
    float hungerStep;

    Transform target = null;
    Vector3 startPos = Vector3.zero;


    float lerpTime;

 
    enum ShrimpStates
    {
        eating,
        dying,
        idling,
        fleeing
    }

    //current state
    ShrimpStates state = ShrimpStates.idling;

    //timer that'll count down for hunger
    float hungerTime;
    //hunger stat
    float hungerVal = 5;

    //list for food currently in the scene
    List<GameObject> allFood = new List<GameObject>();

    //holds which game object the spider has touched
    GameObject touchingObj;

    //could use to display organism stats for debugging. should NOT be in the final game

    public TMP_Text hungerText;


    public Transform fishTransform;

    public FishMovement fishScript;

    [SerializeField] float escapeDistance = 3f;
    [SerializeField] float escapeSpeed = 3f;


    void Start()
    {
        FindAllFood();
        hungerTime = hungerStep;
    }

    void Update()
    {
        StepNeeds();

        if (state == ShrimpStates.dying)
            return;

        if (fishScript != null && fishScript.IsChasingFood())
        {
            float distance = Vector3.Distance(transform.position, fishTransform.position);
            if (distance < escapeDistance)
            {
                state = ShrimpStates.fleeing;
                target = null;
            }
            else if (state == ShrimpStates.fleeing)
            {
                state = ShrimpStates.idling;
            }
        }

        switch (state)
        {
            case ShrimpStates.idling:
                RunIdle();
                break;
            case ShrimpStates.eating:
                RunEat();
                break;
            case ShrimpStates.dying:
                break;
            case ShrimpStates.fleeing:
                RunAwayFromFish();
                break;
        }
     
    }
 

    void RunIdle()
    {
       
        if (hungerVal <= 3) 
        {
            target = null; 
            state = ShrimpStates.eating;
            return;
        }

      
        if (target == null)
        {
          
            float x = Random.Range(-5f, 5f);
            float y = Random.Range(-3f, 3f);
            Vector3 randomPos = new Vector3(x, y, 0);

            GameObject wanderPoint = new GameObject("WanderPoint");
            wanderPoint.transform.position = randomPos;

            target = wanderPoint.transform;
            startPos = transform.position;
            lerpTime = 0;
        }
        else
        {
           
            transform.position = Move();

            // Arrived at wander point?
            if (lerpTime >= lerpTimeMax)
            {
                Destroy(target.gameObject);
                target = null;
            }
        }
    }

    void RunEat()
    {
        if (target == null)
        {
            target = FindNearest(allFood); 
            if (target == null) 
            {
                state = ShrimpStates.idling;
                return;
            }
            startPos = transform.position;
            lerpTime = 0;
        }
        else
        {
            transform.position = Move();

            if (touchingObj != null && touchingObj.CompareTag("seaweed"))
            {
                allFood.Remove(touchingObj);
                hungerVal = 5; 
                Destroy(touchingObj);

                touchingObj = null;
                target = null;
                state = ShrimpStates.idling;
            }
        }
    }

    void StepNeeds()
    {
        hungerTime -= Time.deltaTime; //deincrement the hunger timer
        if (hungerTime <= 0)
        { //if the hunger timer gets to 0
            hungerVal--; //decrease our hunger stat
            hungerTime = hungerStep; //reset the hunger timer

            if(hungerVal <= 0 && state != ShrimpStates.dying)
            {
                state = ShrimpStates.dying;
                Destroy(gameObject, 1f);

            }
        }
    }

   

    void FindAllFood()
    {
        allFood.AddRange(GameObject.FindGameObjectsWithTag("seaweed")); 
    }

    Transform FindNearest(List<GameObject> objsToFind)
    {
        float minDist = Mathf.Infinity;
        Transform nearest = null;

        for (int i = 0; i < objsToFind.Count; i++)
        {
            GameObject obj = objsToFind[i];

            // Skip if the object is null or has been destroyed
            if (obj == null) continue;

            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = obj.transform;
            }
        }

        return nearest;
    }

    Vector3 Move()
    {
        lerpTime += Time.deltaTime;
        float percent = idleWalkCurve.Evaluate(lerpTime / lerpTimeMax);
        Vector3 newPos = Vector3.LerpUnclamped(startPos, target.position, percent);
        return newPos; 
    }

    void RunAwayFromFish()
    {
        Vector3 awayDirection = (transform.position - fishTransform.position).normalized;
        Vector3 newPos = transform.position + awayDirection * escapeSpeed * Time.deltaTime;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        transform.position = newPos;
    }

    public bool IsChasingFood()
    {
        return state == ShrimpStates.eating;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null) touchingObj = col.gameObject; 
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col != null)
        {
            if (col.gameObject == touchingObj) touchingObj = null; 
        }
    }
}

