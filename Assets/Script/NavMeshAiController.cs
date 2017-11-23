using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshAiController : MonoBehaviour {

    private GameObject ball;
    private BalleController ballInfo;
    private NavMeshAgent agent;
    private float xBegin;
    private float zBegin;
    private float xEnd;
    private float zEnd;
    private Vector3 myZoneCenter;

    // Use this for initialization
    void Start () {
        ball = GameObject.FindGameObjectWithTag("ball");
        ballInfo = ball.GetComponent<BalleController>();
        agent = GetComponent<NavMeshAgent>();
        
        initializeMyGameZone();
        myZoneCenter = transform.position;
    }
    private void initializeMyGameZone()  {
        GameObject murs = GameObject.FindGameObjectsWithTag("backgroundMurs")[0];

        for (int i = 0; i < murs.transform.childCount; i++){
            Transform child = murs.transform.GetChild(i);
            if (child.CompareTag("murOuest")){
                xBegin = child.position.x;
            }
            else if (child.CompareTag("murEst")){
                xEnd = child.position.x;
            }
            else if (child.CompareTag("murSud")){
                zBegin = child.position.z;
            }
            else if (child.CompareTag("murNord")){
                zEnd = child.position.z;
            }
        }
        xBegin = (xBegin + xEnd) / 2;
    }

    // Update is called once per frame
    void Update () {
        if (ball == null) {
            ball = GameObject.FindGameObjectWithTag("ball");
            ballInfo = ball.GetComponent<BalleController>();
        }
        Vector3 ballPosition = ball.transform.position;
        ballPosition.y = 0;
        if (ballInMyZone(ballPosition)) {
            if (ballOnMyLeftSide(ballPosition)){
                float distanceToBall = Vector3.Distance(transform.position, ballPosition);
                float speed = distanceToBall * 10;
                if (speed <= 10)
                {
                    speed = 10;
                }
                agent.speed = speed;
                agent.SetDestination(ballPosition - ball.transform.up * 2);
            }
            else {
                goToRightSide(ballPosition);
            }
            
        }
        else {
            if (transform.position != myZoneCenter) {
                goBackToCenter();
            }
        }
        
	}

    private void goToRightSide(Vector3 ballPosition) {
        float offset = 0;
        if (Mathf.Approximately(ballPosition.x, transform.position.x)) {
            if (transform.position.x > (xEnd + xBegin) / 2) {
                //if im on top part i go from bottom side
                offset = -2;
            }
            else {
                //if im bottom part, i go from top side
                offset = 2;
            }
            
        }
        agent.SetDestination(new Vector3(transform.position.x + offset, transform.position.y,ballPosition.z + 2));
    }

    private bool ballOnMyLeftSide(Vector3 ballPosition) {
        if (transform.position.z > ballPosition.z) {
            return true;
        }
        return false;
    }

    private void goBackToCenter() {
        if (agent.speed != 3.5) {
            agent.speed = 3.5f;
            agent.SetDestination(myZoneCenter);
        }
    }

    private bool ballInMyZone(Vector3 ballPosition) {
        if (ballPosition.x <= xEnd && ballPosition.x >= xBegin && ballPosition.z >= zBegin && ballPosition.z <= zEnd) {
            return true;
        }
        return false;
    }
}
