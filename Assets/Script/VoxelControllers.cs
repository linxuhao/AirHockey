using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelControllers : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay(Collider other) {
        ButtonController button = other.GetComponent<ButtonController>();
        if (button) {
            button.addPression();
            button.receiveZPosition(transform.position.z);
        }
    }
}
