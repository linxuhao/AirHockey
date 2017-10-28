using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalleController : MonoBehaviour {

    public bool pushTheBallOnStart;
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
        rb = GetComponent<Rigidbody>();

        speed = 500f;
        Direction = new Vector3(1f, 0.0f, 0.8f);

        if (pushTheBallOnStart) {
            rb.AddForce(speed * direction);
        }
        
    }
	
	// Update is called once per frame
	void LateUpdate () {
        
		
	}

    void OnCollisionEnter(Collision col){
        string colliderTag = col.collider.gameObject.tag;
        if (colliderTag.Equals("Player") || colliderTag.Equals("murNord") || colliderTag.Equals("murSud") || colliderTag.Equals("murOuest") || colliderTag.Equals("murEst"))
        {
            ContactPoint contact = col.contacts[0];
            float angle = Vector3.Angle(contact.normal, Vector3.forward);

            if (Mathf.Approximately(angle, 0)){// top
                choc(speed, Direction.x, -Direction.z);
                //Debug.Log("[BallController] - ball is at top of object");
            }
            if (Mathf.Approximately(angle, 180)){// bottom
                choc(speed, Direction.x, -Direction.z);
                //Debug.Log("[BallController] - ball is at bot of object");
            }
            if (Mathf.Approximately(angle, 90))
            {
                Vector3 cross = Vector3.Cross(Vector3.forward, contact.normal);
                if (cross.y > 0){// Right
                    choc(speed, -Direction.x, Direction.z);
                    //Debug.Log("[BallController] - ball is at right of object");
                } 
                else{// left
                    choc(speed, -Direction.x, Direction.z);
                    //Debug.Log("[BallController] - ball is at left of object");
                }
            }
        } 
    }

    private void choc(float force, float xDirection, float yDirection) {
        Direction = new Vector3(xDirection, 0.0f, yDirection);
        rb.AddForce(force * direction);
    }
}
