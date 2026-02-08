using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

public class EnemyMovement : MonoBehaviour
{
    private Rigidbody2D myBody;
    [SerializeField] private float health = 10f;
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float reachDistance = 0.05f;
    private int currWaypointIndex = 0;
    //[SerializeField]
    //private float playerDetectRange = 5;
    private EnemyPathing targetScript;
    [SerializeField] private bool togglePathDist = false;
    [SerializeField] private float functionDelaySeconds = 2f;
    private float finalDist;
    private int inspectIndex;
    
    void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        GameObject targetObject = GameObject.Find("PathFinding"); //Must name the PathFinding object; I should probably change this to serialized field to ease the process
        if (targetObject != null)
        {
            targetScript = targetObject.GetComponent<EnemyPathing>();
            if(targetScript != null)
            {
                //Minus 1 is used to stay within the list bounds
                currWaypointIndex = targetScript.transformPoints.Count - 1; //Due to how A* search works, it back tracks from the end to the start, meaning the waypoints in the list are backwards, hence we start at the end of the list
            }
        }

        if (togglePathDist)
        {
            StartCoroutine(DisplayPathDist());
        }
    }

    IEnumerator DisplayPathDist()
    {
        while (true)
        {
            Debug.Log(GetPathDist());
            yield return new WaitForSeconds(functionDelaySeconds);
        }
    }


    void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }
        Move(); //Move is used since Update cannot be disabled; Enabled set to false stops enemy movement
    }

    private void Move()
    {
        //Set the direction and movePosition towards the "next" element in the list   
        Vector3 direction = (targetScript.transformPoints[currWaypointIndex].position - transform.position).normalized;
        Vector3 movePosition = transform.position + speed * Time.deltaTime * direction;
        myBody.MovePosition(movePosition);
        Vector3 targetPos = targetScript.transformPoints[currWaypointIndex].position;

        //transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < reachDistance)
        {
            //Decrease index to properly traverse the intended path
            currWaypointIndex--;

            if (currWaypointIndex < 0)
            {
                Debug.Log("EndReached");
                enabled = false; //stops moving the object by disabling the Move function
                //perhaps despawn the enemy
                Destroy(gameObject);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    public float GetPathDist()
    {
        //reset finalDist and inspectIndex variables
        finalDist = 0;
        inspectIndex = currWaypointIndex;

        if(inspectIndex > 0)
        {
            //Calculate distance between current point and next waypoint
            finalDist += Vector3.Distance(transform.position, targetScript.transformPoints[inspectIndex].position);
            //Then find the sum of the remaining unhit checkpoints
            for(int i = inspectIndex; i > 0; i--)
            {
                finalDist += Vector3.Distance(targetScript.transformPoints[i].position, targetScript.transformPoints[i-1].position);
            }
        }
        return finalDist;
    }
}
