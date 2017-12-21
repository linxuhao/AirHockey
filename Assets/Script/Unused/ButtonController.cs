using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ButtonController : MonoBehaviour {

    public GameObject player;
    private PlayerController playerControl;
    //for Z axis mouvement
    private float[][] oldZPosition;
    private float currentZPosition;
    //to calculate a average z position as a fake center
    private float totalZPosition;
    private float zPositionCount;
    //for X axis mouvement
    private float[][] oldPressionValue;
    private float currentPressionValue;
    //the percentage of value we use from current frame
    private float alpha;
    private bool initialized = false;

    // Use this for initialization
    void Start (){

    }

    private void init(){
        //if player not active
        if (player.activeSelf){
            playerControl = player.GetComponent<PlayerController>();

            oldZPosition = new float[webCamStreamIn.instance.smoothStrengh + 1][];
            oldZPosition[0] = new float[webCamStreamIn.instance.frameMemoryNumber];
            oldPressionValue = new float[webCamStreamIn.instance.smoothStrengh + 1][];
            oldPressionValue[0] = new float[webCamStreamIn.instance.frameMemoryNumber];
            //for each strengh, add a array
            for (int i = 1; i <= webCamStreamIn.instance.smoothStrengh; i++){
                oldZPosition[i] = new float[webCamStreamIn.instance.frameMemoryNumber];
                oldPressionValue[i] = new float[webCamStreamIn.instance.frameMemoryNumber];
            }


            resetData();
            setOldFrameData(currentPressionValue, oldPressionValue[0]);
            setOldFrameData(currentZPosition, oldZPosition[0]);
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

        if (isActiveAndEnabled) {
            alpha = webCamStreamIn.instance.currentFrameValuePercentage;

            setOldFrameData(currentPressionValue, oldPressionValue[0]);
            //Exponential moving average
            float xCurrentMovingAverage = getMovingAverage(alpha, oldPressionValue[0]);

            for (int i = 1; i < webCamStreamIn.instance.smoothStrengh; i++){
                //set current data
                setOldFrameData(xCurrentMovingAverage, oldPressionValue[i]);
                xCurrentMovingAverage = getMovingAverage(alpha, oldPressionValue[i]);
            }

            //head of array at 0
            float xValue = xCurrentMovingAverage - oldPressionValue[webCamStreamIn.instance.smoothStrengh][0];
            setOldFrameData(xCurrentMovingAverage, oldPressionValue[webCamStreamIn.instance.smoothStrengh]);


            //to never divide by 0
            if (zPositionCount == 0){
                zPositionCount = 1;
            }
            //current averge position of all pixel, the "center" of object
            currentZPosition = totalZPosition / zPositionCount;
            //set current data
            setOldFrameData(currentZPosition, oldZPosition[0]);
            float zCurrentMovingAverage = getMovingAverage(alpha, oldZPosition[0]);
            for (int i = 1; i < webCamStreamIn.instance.smoothStrengh; i++){
                //set current data
                setOldFrameData(zCurrentMovingAverage, oldZPosition[i]);
                zCurrentMovingAverage = getMovingAverage(alpha, oldZPosition[i]);
            }

            float zValue = zCurrentMovingAverage - oldZPosition[webCamStreamIn.instance.smoothStrengh][0];
            setOldFrameData(zCurrentMovingAverage, oldZPosition[webCamStreamIn.instance.smoothStrengh]);


            //give the calculated mouvement to the player
            playerControl.receiveTranslation(this, xValue, zValue);
            //set/reset data for next frame

            resetData();
        }
    }

    private float getMovingAverage(float alpha, float[] values){
        float result = 0;
        float pow = 0;
        for (int i = 0; i < values.Length - 1; i++) {
            if (values[i] != 0) {
                float currentVariation = alpha * Mathf.Pow((1 - alpha), pow) * values[i];
                result += currentVariation;
                pow++;
            }
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
    /// head at 0
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
