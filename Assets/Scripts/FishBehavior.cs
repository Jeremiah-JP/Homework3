using UnityEngine;
using TMPro;
using System.Collections.Generic;


public class FishBehavior : MonoBehaviour
{


    [SerializeField]
    Transform[] possibleTargets; //array of spots the spider can idle between

    [SerializeField]
    float lerpTimeMax; //appr. how long we want a lerp to last

    [SerializeField]
    AnimationCurve idleWalkCurve; //anim curve to add easing to the lerp

    [SerializeField]
    float hungerStep; //max time we want to wait to deincrement hunger

    Transform target = null; //current spot we're moving towards
    Vector3 startPos = Vector3.zero; //current spot we're moving from

    //count of current lerp progress
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
    // [SerializeField]
    // TMP_Text hungerText;

    void Start()
    {
        FindAllFood(); //find all food objs in the scene
        hungerTime = hungerStep; //reset our hunger timer
    }

    void Update()
    {
        //hungerText.text = hungerVal.ToString();
        //switch statement for our statement
        //cleaner way of checking the same condition
        switch (state)
        {
            case SpiderStates.idling: //if we're in the idle state
                RunIdle(); //run idle code
                break;
            case SpiderStates.eating: //if we're in the eating state
                RunEat(); //run eating code
                break;
            case SpiderStates.showering:
                break;
            case SpiderStates.dying:
                break;
            default:
                break;
        }
    }

    void RunIdle()
    {
        if (target == null)
        { //if we do not have a target to move to
            int newTarget = Random.Range(0, possibleTargets.Length); //find random position
            target = possibleTargets[newTarget]; //set target to that position
            startPos = transform.position; //set our starting pos to our current pos
            lerpTime = 0; //reset our lerp progress
        }
        else
        {
            transform.position = Move(); //move to that position
        }
        StepNeeds(); //increment stat timers
        if (hungerVal <= 0)
        { //if our spider is hunger
            target = null; //remove whatever target we were moving towards
            //state = SpiderStates.eating; //switch the state to eating
        }
        //TO DO: find better thing to do while idle
        //Need to make all behavior be intentional
    }

    void RunEat()
    {
        if (target == null)
        { //if we do not have a target to move to
            target = FindNearest(allFood); //find the closest food obj and set our target to it
            startPos = transform.position; //set our starting pos to our current pos
            lerpTime = 0; //reset our lerp progress
        }
        else
        {
            transform.position = Move(); //move to food
            if (touchingObj != null)
            { //if we're touching a game object
                if (touchingObj.tag == "food")
                {  //and that object is tagged as food
                    allFood.Remove(touchingObj); //remove that food from the food list
                    hungerVal = 5; //reset our hunger
                    Destroy(touchingObj); //destroy that food
                    touchingObj = null; //empty our food tracking variable
                    target = null; //empty our movement target
                    state = SpiderStates.idling; //switch the state to idle
                }
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
