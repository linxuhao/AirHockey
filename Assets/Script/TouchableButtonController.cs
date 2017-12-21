using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchableButtonController : MonoBehaviour {

    private bool touched;
    public GameObject buttonImage;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool isTouched(){
        return touched;
    }

    public void desactive(){
        gameObject.SetActive(false);
        buttonImage.SetActive(false);
        touched = false;
    }

    public void active() {
        gameObject.SetActive(true);
        buttonImage.SetActive(true);
        touched = false;
    }

    public bool isActive() {
        return gameObject.activeSelf;
    }

    void OnTriggerEnter(Collider other){
        if (other.CompareTag("voxel")){
            touched = true;
        }
    }

}
