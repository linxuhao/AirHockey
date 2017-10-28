using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class webCamStreamIn : MonoBehaviour {
    public enum inputMode {
        cameraSimulation,
        webcam
    }
    public inputMode videoInputMode;

    //to display content of webcam on a material
    private Renderer rend;

    //webcam
    private WebCamTexture webcam;
    private int webCamHeight;
    private int webCamWidth;
    private Color[] webcamFrame;

    //webcam simulation camera
    private Camera webcamSimuCam;
    private RenderTexture webcamSimuCamTexture;
    Texture2D webcamSimuCam2d;

    //camera
    private Camera cam;
    private RenderTexture camTexture;
    Texture2D cam2d;
    private int camHeight;
    private int camWidth;
    private Color[] camFrame;

    //new display texture 
    private Texture2D processedWebcamView;

    //image processing attributs
    //color tolerance
    /// <summary>
    /// ////////////////////////////////////////////// Public variables //////////////////////////////////////
    /// </summary>
    public float grayscaletolerance;
    public int webcamResolutionWidth;
    public int webcamResolutionHeight;
    //hashset because it's add and contain's complexity is o(1) and ignore duplicated elements
    private HashSet<Color> backgroundColors;
    //both in seconds
    private float nextRefreshTime = -0.01f;
    private float refreshFrequency = 1.0f;
   
    // Rate of frame per image process, lower is more accurate but cost in performance.
    /// <summary>
    /// ////////////////////////////// Public variables
    /// </summary>
    public int imageProcessRate;
    private int imageProcessCycle;

    //grayscale debug loging attributs

    private int count = 0;
    private int max = 1000;

    // Use this for initialization
    void Start ()
    {
        if (imageProcessRate <= 0){
            Debug.LogError("Please specify imageProcessRate, it is Rate of frame per image process, lower is more accurate but cost in performance.");
        }

        if (grayscaletolerance <= 0){
            Debug.LogError("Please specify grayscaletolerance, it is used to distinguish a border pixel from others.");
        }

        if (webcamResolutionWidth <= 0){
            Debug.LogError("Please specify webcam Resolution Width");
        }

        if (webcamResolutionHeight <= 0){
            Debug.LogError("Please specify webcam Resolution Height");
        }

        rend = GetComponent<Renderer>();

        if (videoInputMode == inputMode.webcam){
            initializeWebcam();
        }
        else
        if (videoInputMode == inputMode.cameraSimulation){
            initializeWebcamSimulationCamera();
        }

        webcamFrame = new Color[webCamHeight * webCamWidth];

        //create a new 2d texture to display processed web cam view
        processedWebcamView = new Texture2D(webCamWidth, webCamHeight);


        //display webcam content in this object's material, the content are processed per frame in update methode.
        //rend.material.mainTexture = webcam;
        rend.material.mainTexture = processedWebcamView;

        initializeBackgroundCamera();

        Debug.Log("cam width : " + camWidth);
        Debug.Log("cam height : " + camHeight);
        //initialize the first frame to process state
        imageProcessCycle = imageProcessRate;
    }


    private void initializeBackgroundCamera()
    {
        //get the background camera
        cam = GameObject.FindGameObjectWithTag("backgroundCamera").GetComponent<Camera>();
        camTexture = cam.targetTexture;

        //create pixel array to save main camera (in game) frames
        camHeight = camTexture.height;
        camWidth = camTexture.width;
        cam2d = new Texture2D(camWidth, camHeight, TextureFormat.RGBA32, true);
        camFrame = new Color[camHeight * camWidth];
    }

    private void initializeWebcamSimulationCamera()
    {
        //get the webcam simulation camera
        webcamSimuCam = GameObject.FindGameObjectWithTag("webcamSimulationCamera").GetComponent<Camera>();
        //create the texture as the camera target
        webcamSimuCamTexture = new RenderTexture(webcamResolutionWidth, webcamResolutionHeight, 0);
        //set the camera target to the newly created texture
        webcamSimuCam.targetTexture = webcamSimuCamTexture;

        //create pixel array to save main camera (in game) frames
        webCamHeight = webcamSimuCamTexture.height;
        webCamWidth = webcamSimuCamTexture.width;
        //create texture to get input pixels
        webcamSimuCam2d = new Texture2D(webCamWidth, webCamHeight, TextureFormat.RGBA32, true);

        Debug.Log("simulated webcam width : " + webCamWidth);
        Debug.Log("simulated webcam height : " + webCamHeight);
    }

    private void initializeWebcam() {
        string webCamName = null;

        //find webcams
        WebCamDevice[] devices = WebCamTexture.devices;
        for (var i = 0; i < devices.Length; i++) {
            //Debug.Log("webcam found : " + devices[i].name);
            webCamName = devices[i].name;
        }

        //initialize the last found webcam(i got only one so) and play
        webcam = new WebCamTexture(webCamName);
        webcam.requestedHeight = webcamResolutionWidth;
        webcam.requestedWidth = webcamResolutionWidth;
        webcam.Play();

        //create pixel array to save webcam (real world) frames
        webCamHeight = webcam.height;
        webCamWidth = webcam.width;

        Debug.Log("webcam width : " + webCamWidth);
        Debug.Log("webcam height : " + webCamHeight);
    }

    // Update is called once per frame
    void Update () {
        //if in game main background camera does exist and be active
        if (cam != null && cam.isActiveAndEnabled) {
            //get webcam pixel array
            webcamFrame = getWebcamPixels();

            //get in game main camera pixel array
            RenderTexture.active = camTexture;
            cam2d.ReadPixels(new Rect(0, 0, camWidth, camHeight), 0, 0);
            cam2d.Apply();
            camFrame = cam2d.GetPixels();

            if (Time.time > nextRefreshTime) {
                nextRefreshTime += refreshFrequency;
                //process on camera background colors
                backgroundColors = getBackgroundColors();
                //Debug.Log("there are " + backgroundColors.Count + " differents backgroundColors");  
            }

            if (imageProcessCycle >= imageProcessRate)
            {
                imageProcessCycle = 1;
                //process on webcam frame
                for (int i = 0; i < webcamFrame.Length; i++)
                {
                    //if is not a background and is a border
                    if (!backgroundColors.Contains(webcamFrame[i]) && isBorder(webcamFrame, webCamWidth, webCamHeight, i))
                    {
                        //mark this pixel to red
                        webcamFrame[i] = Color.red;
                    }
                }
            }
            else {
                imageProcessCycle++;
            }

            processedWebcamView.SetPixels(webcamFrame);
            processedWebcamView.Apply();
        }
        else {
            Debug.LogError("there is no active main  background camera in game, check if your camera is tagged as backgroundCamera");
        }
    }

    private Color[] getWebcamPixels(){
        Color[] frames = null;
        if (videoInputMode == inputMode.webcam) {
            frames = webcam.GetPixels();
        }else
        if (videoInputMode == inputMode.cameraSimulation) {
            RenderTexture.active = webcamSimuCamTexture;
            //webcamSimuCam.Render();
            webcamSimuCam2d.ReadPixels(new Rect(0, 0, webCamWidth, webCamHeight), 0, 0);
            webcamSimuCam2d.Apply();
            frames = webcamSimuCam2d.GetPixels();
        }
        return frames;
    }

    private HashSet<Color> getBackgroundColors() {
        HashSet<Color> result = new HashSet<Color>();
        //process background from camera
        for (int i = 0; i < camFrame.Length; i++) {
            //add operation success only if element not already exist in set
            result.Add(camFrame[i]);
        }
        return result;
    }


    //test if color 1 has big grayscale with color2
    private bool hasBigGrayscaleDifference(Color color1, Color color2, float tolerence) {
        float grayscale1 = color1.grayscale;
        float grayscale2 = color2.grayscale;
        float result = Mathf.Abs(grayscale1 - grayscale2);
        //if (count < max && result != 0)
        //{
        //    count++;
        //    Debug.Log("grayscale 1 : " + grayscale1 + ", grayscale 2 : " + grayscale2 + ", the difference is : " + result);
        //}
        return result > tolerence;
    }

    private bool isBorder(Color[] pixelArray, int imageWidth, int imageHeight, int pixelPosition) {
        bool result = true;
        //check top and bottom
        for (int i = pixelPosition - imageWidth; i <= pixelPosition + imageWidth; i += imageWidth) {
            //check left and right
            for (int j = i - 1; j <= i + 1; j++) {
                //begins with j = pixelPosition - imageWidth -1 ends a*on pixelPosition + imageWidth + 1, 9 possible values, the center is self
                if (j>0 && j < pixelArray.Length && j != pixelPosition && !hasBigGrayscaleDifference(pixelArray[j],pixelArray[pixelPosition], grayscaletolerance)) {
                    // does not have Big Grayscale Difference with nearby pixles means is not a a border
                    return false;
                }
            }
        }
        return result;
    }
}