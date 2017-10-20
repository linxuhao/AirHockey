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
    public GameObject player;

    //ArToolKit functions
    void OnMarkerFound(ARMarker marker)
    {
        Debug.Log("[PlayerController] - MARKER FOUND! YEAHH! --------------------------------------------------------------------------------");
    }

    void OnMarkerLost(ARMarker marker)
    {
        Debug.Log("[PlayerController] - MARKER LOST! FKKKKKK! --------------------------------------------------------------------------------");
    }

    void OnMarkerTracked(ARMarker marker)
    {
        //player.transform.position = marker.
        Debug.Log("[PlayerController] - MARKER TRACKED! WHEEEE! --------------------------------------------------------------------------------");
    }

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update(){
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, 10.0f));
        //Debug.Log("[PlayerController] - mousse position x : " + mousePosition.x + ", mousse position Z : " + mousePosition.z);
        if (mouseInDefinedPlayZone(mousePosition.x, mousePosition.z))
        {
            mousePosition.y = transform.position.y;
            transform.position = mousePosition;

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
