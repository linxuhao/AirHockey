using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour {

	private Vector3 position = new Vector3(0,0.15f,-18);
	public GameObject table;
	public Rigidbody sphere;
	private int[] score;
	public Text score1;
	public Text score2;

	// Use this for initialization
	void Start () {
		score = new int[]{0,0};
		score1.text = score[0].ToString();
		score2.text = score[1].ToString();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision col){
		string colliderTag = col.collider.gameObject.tag;
		if (colliderTag.Equals ("ball")) {
			//Debug.Log ("Collision avec " + col.gameObject.tag);
			Destroy (col.gameObject);
			//(0 0.15 0)
			//Debug.Log ("Instantiate new ball");
			Rigidbody newball = Instantiate (sphere, position,  Quaternion.identity);
			newball.gameObject.transform.parent = table.transform;
			//newball.transform.localScale = new Vector3(0.125f, 0.05f, 0.5f);
			Debug.Log (newball.position);
			if (this.gameObject.name == "Hole1") {
				score[1]++;
				score2.text = score [1].ToString ();
			} else if (this.gameObject.name == "Hole2") {
				score[0]++;
				score1.text = score [0].ToString ();
			}
		}
	}
}

