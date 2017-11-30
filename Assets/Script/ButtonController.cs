using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ButtonController : MonoBehaviour {

    public GameObject player;
    private PlayerController playerControl;
    //for Z axis mouvement
    private float[] oldZPosition;
    private float currentZPosition;
    //to calculate a average z position as a fake center
    private float totalZPosition;
    private float zPositionCount;
    //for X axis mouvement
    private float[] oldPressionValue;
    private float currentPressionValue;
    //the percentage of value we use from current frame
    private float currentFramePercentage;
    private bool initialized = false;

    // Use this for initialization
    void Start (){

    }

    private void init(){
        //if player not active
        if (player.activeSelf){
            playerControl = player.GetComponent<PlayerController>();
            oldZPosition = new float[webCamStreamIn.instance.frameMemoryNumber];
            oldPressionValue = new float[webCamStreamIn.instance.frameMemoryNumber];
            resetData();
            setOldFrameData(currentPressionValue, oldPressionValue);
            setOldFrameData(currentZPosition, oldZPosition);
        }
        else{
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void LateUpdate (){
        if (!initialized){
            init();
            initialized = true;
        }
        currentFramePercentage = webCamStreamIn.instance.currentFramePercentage;
        //calculate the mouvement from by comparing with old frame's data
        float averageXFromLastFrames = getAverage(oldPressionValue);
        float xValue = currentPressionValue * currentFramePercentage + averageXFromLastFrames * (1-currentFramePercentage) - averageXFromLastFrames;
        if (zPositionCount == 0) {
            zPositionCount = 1;
        }
        float averageZFromLastFrames = getAverage(oldZPosition);
        currentZPosition = totalZPosition / zPositionCount;
        float zValue = currentZPosition * currentFramePercentage + averageZFromLastFrames * (1 - currentFramePercentage) - averageZFromLastFrames;

        //give the calculated mouvement to the player
        playerControl.receiveTranslation(this,xValue, zValue);
        //set/reset data for next frame
        setOldFrameData(currentPressionValue, oldPressionValue);
        setOldFrameData(currentZPosition, oldZPosition);
        resetData();
    }

    private float getAverage(float[] values){
        float result = 0;
        float count = 0;
        for (int i = 0; i < values.Length; i++) {
            if (values[i] != 0) {
                result += values[i];
                count++;
            }
        }
        if (count != 0){
            result /= count;
        }
        return result;
    }

    /// <summary>
    /// reset data after each frame
    /// </summary>
    private void resetData() {
        currentZPosition = 0;
        totalZPosition = 0;
        zPositionCount = 0;
        currentPressionValue = 0;
    }

    /// <summary>
    /// read this function's name
    /// </summary>
    private void setOldFrameData(float currentValue, float[] oldValues) {
        for (int i = oldValues.Length-1; i > 0; i--) {
            oldValues[i] = oldValues[i - 1];
        }
        oldValues[0] = currentValue;
    }

    /// <summary>
    /// add pression for each object on this button, for X axis mouvement
    /// </summary>
    public void addPression() {
        currentPressionValue += 0.01f;
    }

    /// <summary>
    /// add z position to calculate a fake center on Z axis by calculating a average
    /// </summary>
    /// <param name="zPosition"></param>
    public void receiveZPosition(float zPosition) {
        totalZPosition += zPosition;
        zPositionCount++;
    }
}
