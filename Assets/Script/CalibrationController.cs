using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CalibrationController : MonoBehaviour {

    public GameObject mainCamera;

    //GUI 
    public RawImage display;
    public Slider leftoffsetSlider;
    public Slider rightoffsetSlider;
    public Slider topoffsetSlider;
    public Slider botoffsetSlider;
    public Button finishButton;

    private int leftoffset;
    private int rightoffset;
    private int topoffset;
    private int botoffset;

    //Webcam
    private WebCamTexture webcam;
    private int webCamHeight;
    private int webCamWidth;
    private int usableWebCamHeight;
    private int usableWebCamWidth;
    private bool initialized;

    private Texture2D imageToDisplay;

    // Use this for initialization
    void Start ()
    {

        if (mainCamera.activeSelf)
        {
            mainCamera.SetActive(false);
        }
        initialized = false;

        leftoffsetSlider.minValue = 0;
        rightoffsetSlider.minValue = 0;
        topoffsetSlider.minValue = 0;
        botoffsetSlider.minValue = 0;

        leftoffsetSlider.maxValue = 99;
        rightoffsetSlider.maxValue = 99;
        topoffsetSlider.maxValue = 99;
        botoffsetSlider.maxValue = 99;

        leftoffsetSlider.onValueChanged.AddListener(delegate { ValueLeftChangeCheck(); });
        rightoffsetSlider.onValueChanged.AddListener(delegate { ValueRightChangeCheck(); });
        topoffsetSlider.onValueChanged.AddListener(delegate { ValueTopChangeCheck(); });
        botoffsetSlider.onValueChanged.AddListener(delegate { ValueBotChangeCheck(); });
    }

    public void ValueLeftChangeCheck()
    {
        int temp = (int)leftoffsetSlider.value;
        if (temp + rightoffset < 99)
        {
            leftoffset = temp;
            //Debug.Log(leftoffsetSlider.name + " value changed to " + leftoffset);
        }
        else {
            leftoffsetSlider.value = leftoffset;
        }
        
    }

    public void ValueRightChangeCheck()
    {
        int temp = (int)rightoffsetSlider.value;
        if (temp + leftoffset < 99)
        {
            rightoffset = temp;
            //Debug.Log(rightoffsetSlider.name + " value changed to " + rightoffset);
        }
        else
        {
            rightoffsetSlider.value = rightoffset;
        }
    }

    public void ValueTopChangeCheck()
    {
        int temp = (int)topoffsetSlider.value;
        if (temp + botoffset < 99)
        {
            topoffset = temp;
            //Debug.Log(topoffsetSlider.name + " value changed to " + topoffset);
        }
        else
        {
            topoffsetSlider.value = topoffset;
        }
    }

    public void ValueBotChangeCheck()
    {
        int temp = (int)botoffsetSlider.value;
        if (temp + topoffset < 99)
        {
            botoffset = temp;
            //Debug.Log(botoffsetSlider.name + " value changed to " + botoffset);
        }
        else
        {
            botoffsetSlider.value = botoffset;
        }
    }


    private void initSliderValues()
    {
        leftoffset = webCamStreamIn.instance.leftOffset;
        leftoffsetSlider.value = leftoffset;

        rightoffset = webCamStreamIn.instance.rightOffset;
        rightoffsetSlider.value = rightoffset;

        topoffset = webCamStreamIn.instance.topOffset;
        topoffsetSlider.value = topoffset;

        botoffset = webCamStreamIn.instance.botOffset;
        botoffsetSlider.value = botoffset;
    }

    private bool initializeWebcam(){
        if(!webcam)
        {
            string webCamName = null;
            //find webcams
            WebCamDevice[] devices = WebCamTexture.devices;
            for (var i = 0; i < devices.Length; i++)
            {
                //Debug.Log("webcam found : " + devices[i].name);
                webCamName = devices[i].name;
            }
            //initialize the last found webcam(i got only one so) and play
            webcam = new WebCamTexture(webCamName);
            webcam.requestedHeight = webCamStreamIn.instance.webcamResolutionHeight;
            webcam.requestedWidth = webCamStreamIn.instance.webcamResolutionWidth;
            webcam.Play();
        }

        webCamHeight = webcam.height;
        webCamWidth = webcam.width;
        if (webCamHeight < 100 && webCamWidth < 100)
        {
            //Web cam return small number such as 16 as height and width at the beginning
            Debug.Log("webCamHeight: " + webCamHeight + ", webCamWidth: " + webCamWidth + "Waitting for webcam to be rdy");
            return false;
        }
        return true;
    }

    public void finishCalibration(){

        webcam.Stop();

        webCamStreamIn.instance.leftOffset = leftoffset;
        Debug.Log("left offset value changed to " + leftoffset);

        webCamStreamIn.instance.rightOffset = rightoffset;
        Debug.Log("right offset value changed to " + rightoffset);

        webCamStreamIn.instance.topOffset = topoffset;
        Debug.Log("top offset value changed to " + topoffset);

        webCamStreamIn.instance.botOffset = botoffset;
        Debug.Log("bot offset value changed to " + botoffset);

        webCamStreamIn.instance.gameCalibrated = true;
        Debug.Log("calibration finished : " + webCamStreamIn.instance.gameCalibrated);

        gameObject.SetActive(false);

        mainCamera.SetActive(true);

    }

    void Update(){
        if (!initialized){
            initialized = initializeWebcam();
            //initSliderValues();
        }
        else {
            refreshOffset();
            Color[] webcamFrame = getWebcamPixels();
            Texture2D oldtexture = imageToDisplay;
            imageToDisplay = new Texture2D(usableWebCamWidth, usableWebCamHeight);
            imageToDisplay.SetPixels(webcamFrame);
            imageToDisplay.Apply();
            display.texture = imageToDisplay;
            //force garbage collection, because unity is not doing it right 
            if (oldtexture != null){
                Destroy(oldtexture);
            }

        }
        
    }

    private void refreshOffset(){
        usableWebCamHeight = webCamHeight * (100 - topoffset - botoffset) / 100;
        usableWebCamWidth = webCamWidth * (100 - leftoffset - rightoffset) / 100;
    }

    private Color[] getWebcamPixels(){
        Color[] frames = null;
        int xFrom = webCamWidth * leftoffset / 100;
        int yFrom = webCamHeight * topoffset / 100;
        frames = webcam.GetPixels(xFrom, yFrom, usableWebCamWidth, usableWebCamHeight);
        return frames;
    }
}
