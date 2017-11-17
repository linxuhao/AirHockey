using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BalleController : MonoBehaviour {

    public bool pushTheBallOnStart;
    private float speed;
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
    }

    void OnCollisionEnter(Collision collision){
        float tempSpeed = speed;
        //return harder when hitting walls
        GameObject collider = collision.collider.gameObject;
        if (collider.CompareTag("murNord") || collider.CompareTag("murSud") || collider.CompareTag("murOuest") || collider.CompareTag("murEst")) {
            tempSpeed = tempSpeed * 5;
        }
        ContactPoint contact = collision.contacts[0];
        Vector3 contactVector = transform.position - contact.point;
        choc(tempSpeed, contactVector.x, contactVector.z);
    }

    //x and y on 2D plan, is x and z on 3D plan
    public void choc(float force, float xDirection, float yDirection) {
        direction = new Vector3(xDirection, 0, yDirection);
        rb.AddForce(force * direction, ForceMode.Impulse);
    }
}
