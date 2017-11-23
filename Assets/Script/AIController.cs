using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour {

	public GameObject ball;
	private BalleController ballInfo;
	private Vector3 ballPosition;
	private Vector2 insertion;
	private Vector2 ballDirection;
	private Vector2 aiDirection;
	private Rigidbody ballRigid;
	private bool move;
	private float speed = 15f;
	// Use this for initialization
	void Start () {
		ballInfo = ball.GetComponent<BalleController>();
		ballPosition = ball.transform.position;
		move = false;
	}
	
	// Update is called once per frame
	void Update () {
		// when the old ball has been destroyed, it should reinitalize gameobject ball
		if (ball == null) {
			ball = GameObject.FindWithTag ("ball");
		}

		//!!!!!!!!!!!!!!!!ball.z +18


		ballPosition = ball.transform.position;
		ballRigid = ball.GetComponent<Rigidbody> ();
		ballDirection = new Vector2(ballRigid.velocity.x,ballRigid.velocity.z);
		//if the ball enter the attack zone
		if (ballPosition.x>=1f && move == false) {
			/*
			ballPosition:point A, AIPosition: point B
			if AB//axe x
			*/ 
			if (ballDirection.y == 0) {
				Debug.Log ("ballDirection:"+ballDirection);
				move = true;
				aiDirection = -1 * ballDirection.normalized;
				GetComponent<Rigidbody> ().velocity = new Vector3 (aiDirection.x, 0, aiDirection.y) * speed;
			} 
			else {
				/*
				ballPosition:O1(a,b) aiPosition:O2(c,d)
				insertion: (x,y) vBall:(p,q)
				*/
				float a = ballPosition.x;
				float b = ballPosition.z;
				float c = transform.position.x;
				float d = transform.position.y;
				float p = ballDirection.normalized.x;
				float q = ballDirection.normalized.y;
				float distance1;
				float distance2;
				float time;
				move = true;
				//insertion.x = (p * q * (b - d) + p * p * c + q * q * a) / (p * p + q * q);
				//insertion.y = (p * p * (b + d) + p * q * (d + c)) / (p * p + q * q);
				aiDirection = new Vector2 (-q,p);
				Debug.Log ("insertion:"+insertion);
				Debug.Log ("aiDirection:"+aiDirection);
				Debug.Log ("ballDirection:"+ballDirection.normalized);

				//distance
				insertion = new Vector2 (a*1.2f, b*1.2f);
				//Debug.Log (aiDirection * ballDirection);
				distance1 = ballDirection.normalized.magnitude;//1
				distance2 = (insertion - new Vector2 (c, d)).magnitude;
				time = distance1 / ballRigid.velocity.magnitude;
				Debug.Log ("BallPositino"+new Vector2 (a, b));
				Debug.Log ("insertion"+insertion);
				Debug.Log ("distance1"+distance1);
				Debug.Log ("distance2"+distance2);
				Debug.Log (ballRigid.velocity.magnitude);
				speed = distance2 / time;
				GetComponent<Rigidbody> ().velocity = new Vector3 (aiDirection.x, 0, aiDirection.y) * speed;

			}
		}

	}

	void OnCollisionEnter(Collision col){
		string colliderTag = col.collider.gameObject.tag;
		if (colliderTag.Equals ("ball")) {
			//to do: calucate again 
			ballRigid.AddForce (-20f * ballRigid.velocity.normalized,ForceMode.Impulse);
			//AI goes back to start point?
			transform.position=Vector3.MoveTowards(transform.position,new Vector3(2f,0.25f,0),Time.deltaTime*2);
			//Debug.Log (transform.position);
		}
	}
}
