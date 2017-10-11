using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //four point define a play zone which the player can move freely inside
    public float playZoneXBegin;
    public float playZoneXEnd;
    public float playZoneZBegin;
    public float playZoneZEnd;

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update(){
        Vector3 moussePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, 10.0f));
        //Debug.Log("[PlayerController] - mousse position x : " + moussePosition.x + ", mousse position Z : " + moussePosition.z);
        if (mouseInDefinedPlayZone(moussePosition.x, moussePosition.z))
        {
            moussePosition.y = transform.position.y;
            transform.position = moussePosition;

        }
    }

    private bool mouseInDefinedPlayZone(float x, float z)
    {
        if (x > playZoneXBegin && x < playZoneXEnd && z > playZoneZBegin && z < playZoneZEnd) {
            return true;
        }
        return false;
    }
}
