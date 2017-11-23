using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BalleController : MonoBehaviour {

    public bool pushTheBallOnStart;
    public float speed;
    public Vector3 direction;
    private Rigidbody rb;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        speed = 10f;
        direction = new Vector3(1f, 0, 0.8f);

        if (pushTheBallOnStart) {
            choc(speed, direction.x, direction.z);
        }
        
    }

    void Update(){
        //check position instead of using collider when hitting wall
        float x = transform.position.x;
        float z = transform.position.z;
        float tempSpeed = speed;
        if (x > webCamStreamIn.instance.xEnd - 0.5 || x < webCamStreamIn.instance.xBegin + 0.5)
        {

            tempSpeed *= 4;
            choc(tempSpeed, direction.x, -direction.z);

        }
        else if (z > webCamStreamIn.instance.zEnd - 0.5 || z < webCamStreamIn.instance.zBegin + 0.5)
        {
            tempSpeed *= 4;
            choc(tempSpeed, -direction.x, direction.z);
        }

    }

    void OnCollisionEnter(Collision collision){
        float tempSpeed = speed;

        //the center of contact points
        Vector3 contactPoint = new Vector3();
        float x = 0;
        float y = 0;
        float z = 0;
        for (int i = 0; i < collision.contacts.Length; i++) {
            ContactPoint contact = collision.contacts[i];
            x += contact.point.x;
            y += contact.point.y;
            z += contact.point.z;
        }
        x /= collision.contacts.Length;
        y /= collision.contacts.Length;
        z /= collision.contacts.Length;
        contactPoint.x = x;
        contactPoint.y = y;
        contactPoint.z = z;

        Vector3 contactVector = transform.position - contactPoint;
        choc(tempSpeed, contactVector.x, contactVector.z);
        
        
    }

    //x and y on 2D plan, is x and z on 3D plan
    public void choc(float force, float xDirection, float yDirection) {
        direction = new Vector3(xDirection, 0, yDirection);
        rb.AddForce(force * direction, ForceMode.Impulse);
    }
}
