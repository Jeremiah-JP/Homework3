using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class FishBehavior : MonoBehaviour
{
    [SerializeField] float minX = -5f, maxX = 5f;
    [SerializeField] float minY = -3f, maxY = 3f;

    [SerializeField] Transform fishVisual;
    [SerializeField] float wiggleSpeed = 5f;
    [SerializeField] float wiggleAmount = 10f;

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

    //enum is like a custom variable type
    //we're using it to make states for our spider's behavior
    enum SpiderStates
    {
        eating,
        dying,
        idling,
        fleeing
    }

    //current state
    SpiderStates state = SpiderStates.idling;

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

  
   public Transform sharkTransform;

   public SharkBehavior sharkScript;

    [SerializeField] float escapeDistance = 3f;
    [SerializeField] float escapeSpeed = 3f;


    void Start()
    {
        FindAllFood();
        hungerTime = hungerStep;
    }

    void Update()
    {
        if (sharkScript != null && sharkScript.IsChasingFood())
        {
           
            float distance = Vector3.Distance(transform.position, sharkTransform.position);
            if (distance < escapeDistance)
            {
                state = SpiderStates.fleeing;
                target = null;
            }
            else if (state == SpiderStates.fleeing)
            {
                state = SpiderStates.idling; // return to normal if fish is far
            }
        }

        switch (state)
        {
            case SpiderStates.idling:
                RunIdle();
                break;
            case SpiderStates.eating:
                RunEat();
                break;
            case SpiderStates.dying:
                break;
            case SpiderStates.fleeing:
                RunAwayFromFish();
                break;
        }
        Wobble();
        hungerText.text = "Fish Hunger: " + hungerVal.ToString("F1");
    }
     void Wobble()
    {
        float wobble = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmount;
         fishVisual.localRotation = Quaternion.Euler(0, 0, wobble);
     }

    void RunIdle()
    {
       
        if (hungerVal <= 3) 
        {
            target = null; 
            state = SpiderStates.eating;
            return;
        }

        // ?? Normal random wandering
        if (target == null)
        {
            // pick a random position in the aquarium bounds
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

            if (lerpTime >= lerpTimeMax)
            {
                Destroy(target.gameObject); 
                target = null;
            }
        }

        StepNeeds(); 
    }

    void RunEat()
    {
        // Refresh food list
        GameObject[] foundFood = GameObject.FindGameObjectsWithTag("shrimp");
        for (int i = 0; i < foundFood.Length; i++)
        {
            GameObject food = foundFood[i];
            if (!allFood.Contains(food))
            {
                allFood.Add(food);
            }
        }

        if (target == null)
        {
            target = FindNearest(allFood);
            if (target == null)
            {
                state = SpiderStates.idling;
                return;
            }
            startPos = transform.position;
            lerpTime = 0;
        }
        else
        {
            transform.position = Move();

            if (touchingObj != null && touchingObj.CompareTag("shrimp"))
            {
                allFood.Remove(touchingObj);
                hungerVal = 5;
                Destroy(touchingObj);

                touchingObj = null;
                target = null;
                state = SpiderStates.idling;
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
        }
    }

    void FindAllFood()
    {
        allFood.AddRange(GameObject.FindGameObjectsWithTag("shrimp")); //find all objs tagged food and put them in a list
    }

    Transform FindNearest(List<GameObject> objsToFind)
    {
        float minDist = Mathf.Infinity;
        Transform nearest = null;

        for (int i = 0; i < objsToFind.Count; i++)
        {
            GameObject obj = objsToFind[i];

          
            if (obj == null || obj.transform == null) continue;

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
        Vector3 awayDirection = (transform.position - sharkTransform.position).normalized;
        Vector3 newPos = transform.position + awayDirection * escapeSpeed * Time.deltaTime;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        transform.position = newPos;
    }

    public bool IsChasingFood()
    {
        return state == SpiderStates.eating;
    }

    void CleanFoodList()
    {
        for (int i = allFood.Count - 1; i >= 0; i--)
        {
            if (allFood[i] == null)
            {
                allFood.RemoveAt(i);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null) touchingObj = col.gameObject; 
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col != null)
        { //if we stop touching something 
            if (col.gameObject == touchingObj) touchingObj = null; 
        }
    }
}