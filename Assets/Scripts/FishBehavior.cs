using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class FishBehavior : MonoBehaviour
{
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
        showering,
        dying,
        idling
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
     [SerializeField]
     TMP_Text hungerText;

    void Start()
    {
        FindAllFood(); 
        hungerTime = hungerStep; 
    }

    void Update()
    {
        switch (state)
        {
            case SpiderStates.idling:
                RunIdle();
                break;
            case SpiderStates.eating:
                RunEat();
                break;
            case SpiderStates.showering:
                break;
            case SpiderStates.dying:
                break;
        }
        Wobble();
        hungerText.text = "Hunger: " + hungerVal.ToString("F1");
    }
    void Wobble()
    {
        float wobble = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmount;
        fishVisual.localRotation = Quaternion.Euler(0, 0, wobble);
    }

    void RunIdle()
    {
        // ?? If hunger is low, switch to eating state
        if (hungerVal <= 2) // <-- threshold you can tweak
        {
            target = null; // clear current wander target
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
            // Swim toward target
            transform.position = Move();

            // Arrived at wander point?
            if (lerpTime >= lerpTimeMax)
            {
                Destroy(target.gameObject); // clean up wander point
                target = null;
            }
        }

        StepNeeds(); // hunger still goes down
    }

    void RunEat()
    {
        if (target == null)
        {
            target = FindNearest(allFood); // find closest food
            if (target == null) // no food available
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

            if (touchingObj != null && touchingObj.CompareTag("food"))
            {
                allFood.Remove(touchingObj);
                hungerVal = 5; // reset hunger
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
        allFood.AddRange(GameObject.FindGameObjectsWithTag("food")); //find all objs tagged food and put them in a list
    }

    Transform FindNearest(List<GameObject> objsToFind)
    {
        float minDist = Mathf.Infinity; //setting the min dist to a big number
        Transform nearest = null; //tracks the obj closest to us
        for (int i = 0; i < objsToFind.Count; i++)
        { //loop through the objects we're checking
            float dist = Vector3.Distance(transform.position, objsToFind[i].transform.position); //check the dist b/t the spider and the current obj
            if (dist < minDist)
            { //if the dist is less than our currently tracked min dist
                minDist = dist; //set the min dist to the new dist
                nearest = objsToFind[i].transform; //set the nearest obj var to this obj
            }
        }
        return nearest; //return the closest obj
    }

    Vector3 Move()
    {
        lerpTime += Time.deltaTime; //increase progress by delta time (time b/t frames)
        float percent = idleWalkCurve.Evaluate(lerpTime / lerpTimeMax); //from progress on curve
        Vector3 newPos = Vector3.LerpUnclamped(startPos, target.position, percent); //find current lerped position
        return newPos; //return the new position
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col != null) touchingObj = col.gameObject; //if we touch something, set the var to whatever that thing is
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col != null)
        { //if we stop touching something 
            if (col.gameObject == touchingObj) touchingObj = null; //AND that thing is being tracked, clear the touching tracking var
        }
    }
}
