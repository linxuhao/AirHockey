using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision col){
        string colliderTag = col.collider.gameObject.tag;
        if (colliderTag.Equals("ball"))
        {
            ScoreController.instance.playVictory();
            //Debug.Log ("Collision avec " + col.gameObject.tag);
            Destroy(col.gameObject);
            //(0 0.15 0)
            //Debug.Log ("Instantiate new ball");
            StartCoroutine(ScoreController.instance.instanciateNewBall());
            //newball.transform.localScale = new Vector3(0.125f, 0.05f, 0.5f);
            //Debug.Log (newball.position);
            if (this.gameObject.name == "Hole1")
            {
                ScoreController.instance.addScorePlayer1();
            }
            else if (this.gameObject.name == "Hole2")
            {
                ScoreController.instance.addScorePlayer2();
            }
        }
    }
}
