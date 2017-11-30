using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BalleController : MonoBehaviour {

    public bool pushTheBallOnStart;
    public float speed;
    public Vector3 direction;
    private Rigidbody rb;
    private bool onEdge = false;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        direction = new Vector3(1f, 0, 0.8f);

        if (pushTheBallOnStart) {
            choc(speed, direction.x, direction.z);
        }
        
    }

    void Update(){
    }

    void OnCollisionEnter(Collision collision){
        float tempSpeed = speed;
        Collider collider = collision.collider;
        //hit wall is different from hitting objects
        if (collider.CompareTag("murSud") || collider.CompareTag("murNord")){
            choc(tempSpeed, direction.x, -direction.z);
        }
        else if (collider.CompareTag("murEst") || collider.CompareTag("murOuest")){
            choc(tempSpeed, -direction.x, direction.z);
        }
        else
        {
            //the center of contact points
            Vector3 contactPoint = new Vector3();
            float x = 0;
            float y = 0;
            float z = 0;
            for (int i = 0; i < collision.contacts.Length; i++){
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
    }

    //x and y on 2D plan, is x and z on 3D plan
    public void choc(float speed, float xDirection, float yDirection) {
        direction = new Vector3(xDirection, 0, yDirection);
        Vector3 speedVector = new Vector3(direction.normalized.x * speed, 0, direction.normalized.z * speed);
        rb.velocity = speedVector;
    }
}
