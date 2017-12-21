using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour {

	public GameObject ball;
	private Vector3 ballPosition;
	private Vector2 insertion;
	private Vector2 ballDirection;
	private Vector2 aiDirection;
	private Rigidbody ballRigid;
	private Rigidbody aiRigid;
	private const int RAYCASTLENGTH = 100;
	private bool crash;
	public float speed;
	public float ballSpeed;
	// Use this for initialization
	void Start () {
		ballPosition = ball.transform.position;
		ballRigid = ball.GetComponent<Rigidbody> ();
		aiRigid = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		// when the old ball has been destroyed, it should reinitalize gameobject ball
		if (ball == null) {
			ball = GameObject.FindWithTag ("ball");
		}
		ballPosition = ball.transform.position;
		ballRigid = ball.GetComponent<Rigidbody> ();
		aiRigid = GetComponent<Rigidbody> ();
		ballDirection = new Vector2(ballRigid.velocity.x,ballRigid.velocity.z);
		Vector3 dir = new Vector3(ballRigid.velocity.x, 0, ballRigid.velocity.z);

		//Raycasting
		RaycastHit hitInfo;
		Ray ray = new Ray(ballPosition, dir);
		Debug.DrawLine(ballPosition, dir*100, Color.red);
		RaycastHit hit;

		//if the ball enter the attack zone
		if (ballPosition.x >= 1f) {
			/*
			ballPosition:point A, AIPosition: point B
			if AB//axe x
			*/ 
			if (ballDirection.y == 0) {
				Debug.Log ("ballDirection:" + ballDirection);
				aiDirection = -1 * ballDirection.normalized;
				aiRigid.velocity = new Vector3 (aiDirection.x, 0, aiDirection.y) * speed;
			} else {
				/*
				ballPosition:O1(a,b) aiPosition:O2(c,d)
				insertion: (x,y) vBall:(p,q)
				*/
				float a = ballPosition.x;
				float b = ballPosition.z + 18;
				float c = transform.position.x;
				float d = transform.position.y;
				float p = ballDirection.normalized.x;
				float q = ballDirection.normalized.y;
				float distance1;
				float distance2;
				float time;
				//insertion.x = (p * q * (b - d) + p * p * c + q * q * a) / (p * p + q * q);
				//insertion.y = (p * p * (b + d) + p * q * (d + c)) / (p * p + q * q);
				//分类讨论方向
				if (p > 0 && q > 0) {
					if (transform.position.z < ballPosition.z) {
						aiDirection = new Vector2 (-q, p);
					} else {
						aiDirection = new Vector2 (q, -p);
					}
				}else if (p > 0 && q < 0) {
					if (transform.position.z > ballPosition.z) {
						aiDirection = new Vector2 (q, -p);
					} else {
						aiDirection = new Vector2 (-q, p);
					}
				} else{
					aiDirection = new Vector2 (0, 0);
				}
				aiRigid.velocity = new Vector3 (aiDirection.x, 0, aiDirection.y) * speed;
				Debug.Log ("aiDirection:" + aiDirection);
				Debug.Log ("ballDirection:" + ballDirection.normalized);

				bool rayCasted = Physics.Raycast (ray, out hitInfo, RAYCASTLENGTH);
				if (rayCasted) {
					rayCasted = hitInfo.transform.CompareTag ("AI");
					if (rayCasted == true) {
					}
				}
				if (rayCasted) {
					aiRigid.velocity = new Vector3 (0, 0, 0);
				}
				/*
				//distance
				insertion = new Vector2 (a*0.5f, b*0.5f);
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
				*/

			}
		} 
		else {
			float step = speed * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, new Vector3(8f,0.25f,-18f), step);	
		}

	}

	void OnCollisionEnter(Collision col){
		//crash = true;
		string colliderTag = col.collider.gameObject.tag;
		if (colliderTag.Equals ("ball")) {
			//to do: calucate again 
			ballRigid.velocity = new Vector3(ballDirection.x,0,ballDirection.y)*ballSpeed*-1;;
			//AI goes back to start point?
			//transform.position=Vector3.MoveTowards(transform.position,new Vector3(2f,0.25f,-18),Time.deltaTime*2);
			//Debug.Log (transform.position);
		}
	}
}
