using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalleController : MonoBehaviour {

    private float speed;
    private Vector3 direction;
    private Rigidbody rb;

    public float Speed
    {
        get
        {
            return speed;
        }

        set
        {
            speed = value;
        }
    }

    public Vector3 Direction
    {
        get
        {
            return direction;
        }

        set
        {
            direction = value;
        }
    }


    // Use this for initialization
    void Start () {
        speed = 500f;
        Direction = new Vector3(1f,0.0f,0.8f);
        rb = GetComponent<Rigidbody>();
        rb.AddForce(speed * direction);
    }
	
	// Update is called once per frame
	void LateUpdate () {
        
		
	}

    void OnCollisionEnter(Collision col){
        if (col.collider.gameObject.CompareTag("murNord")) {
            choc(speed, Direction.x, -Direction.z);
        }
        if (col.collider.gameObject.CompareTag("murSud"))
        {
            choc(speed, Direction.x, -Direction.z);
        }
        if (col.collider.gameObject.CompareTag("murOuest"))
        {
            choc(speed, -Direction.x, Direction.z);
        }
        if (col.collider.gameObject.CompareTag("murEst"))
        {
            choc(speed, -Direction.x, Direction.z);
        }
    }

    private void choc(float force, float xDirection, float yDirection) {
        Direction = new Vector3(xDirection, 0.0f, yDirection);
        rb.AddForce(force * direction);
    }
}
