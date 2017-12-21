using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //four point define a play zone which the player can move freely inside
    private double playZoneXBegin;
    private double playZoneXEnd;
    private double playZoneZBegin;
    private double playZoneZEnd;
    private bool initialized = false;
    private Vector3 translation;
    //the mouvement speed pf player on x and z axis
    public float xSpeed;
    public float zSpeed;
    //the direction on x axis, player 1 is positiven(x++ means forward) while player 2 is negative(x-- means forward)
    private int xDirection;

    // Use this for initialization
    void Start (){
    }

    private void init() {
        //intialize player play zone (the zone they can't leave)
        double middle = (webCamStreamIn.instance.xBegin + webCamStreamIn.instance.xEnd) * 0.5;
        //left, means p1
        if (transform.position.x < middle)
        {
            playZoneXBegin = webCamStreamIn.instance.xBegin + 0.5 * transform.localScale.x;
            playZoneXEnd = middle + 1 + 0.5 * transform.localScale.x;
            xDirection = 1;
        }
        else
        {
            //right means p2
            playZoneXBegin = middle - 1 - 0.5 * transform.localScale.x;
            playZoneXEnd = webCamStreamIn.instance.xEnd - 0.5 * transform.localScale.x;
            xDirection = -1;
        }
        playZoneZBegin = webCamStreamIn.instance.zBegin + 0.5 * transform.localScale.z;
        playZoneZEnd = webCamStreamIn.instance.zEnd - 0.5 * transform.localScale.z;
        translation = new Vector3();
    }

    // Update is called once per frame
    void Update(){
        if (!initialized) {
            init();
            initialized = true;
        }

        Vector3 myPosition = transform.position;
        //make sure player stay in play zone
        translation = nextPositionInDefinedPlayZone(myPosition.x, myPosition.z, translation);
        translation.Set(translation.x * xSpeed, translation.y, translation.z * zSpeed);
        transform.Translate(translation * Time.deltaTime);
        
        translation = new Vector3();
        //reset translation each frame
    }

    /// <summary>
    /// make sure next position in playZone
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="translation"></param>
    /// <returns>the new calculatedd position</returns>
    private Vector3 nextPositionInDefinedPlayZone(float x, float z, Vector3 translation) {
        Vector3 result = translation;
        float nextXPosition = x + translation.x * Time.deltaTime * xSpeed;
        float nextZPosition = z + translation.z * Time.deltaTime * zSpeed;
        //if next x is out of play zone, reduce x to 0
        if (nextXPosition < playZoneXBegin || nextXPosition > playZoneXEnd) {
            result.x = 0;
        }
        if (nextZPosition < playZoneZBegin || nextZPosition > playZoneZEnd) {
            //if next z is out of play zone, reduce x to 0
            result.z= 0;
        }
        return result;
    }

    /// <summary>
    /// receive the mouvement value, need to be called each frame
    /// </summary>
    /// <param name="xValue"></param>
    /// <param name="zValue"></param>
    public void receiveTranslation(ButtonController button, float xValue, float zValue) {
        translation.x += xValue * xDirection;
        translation.z += zValue;
    }
}
