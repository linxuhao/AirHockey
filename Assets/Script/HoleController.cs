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
            ScoreController.instance.ball = null;
            Destroy(col.gameObject);
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
