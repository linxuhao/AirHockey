using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class webCamStreamIn : MonoBehaviour {
    public enum outputMode {
        debug,
        game
    }
    [Header("Camera input options")]
    [Tooltip("Invert X and Y from webcam")]
    public bool invertAxisXYFromCamera;

    //to display content of webcam on a material
    private Renderer rend;

    //webcam
    private WebCamTexture webcam;
    private int webCamHeight;
    private int webCamWidth;
    private Color[] webcamFrame;

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
    /// ////////////////////////////////////////////// Public variables //////////////////////////////////////
    [Header("Image Processing Attributes")]

    [Tooltip("level from 0(not filtering at all) to 6(filter all)")]
    public int noiseFilteringLevel;
    [Tooltip("is difference between two grayscaled colors in %")]
    public float grayscaletolerance;
    [Tooltip("webcam Resolution Width")]
    public int webcamResolutionWidth;
    [Tooltip("webcam Resolution Height")]
    public int webcamResolutionHeight;

    private Color colorTrackingMark = Color.green;
    
    //in seconds, -0.01f to begin the refresh on the first frame
    private float nextRefreshTime = -0.01f;
    //in seconds
    private float backgroundColorRefreshFrequency = 1.0f;

    ////////////////////////////// Public variables  //////////////////////////////

    [Tooltip("Rate of frame per image process, lower is more accurate but cost in performance. from 1 to infinite....")]
    public int imageProcessRate;
    private int imageProcessCycle;

    //game zone attributs
    float xBegin;
    float xEnd;
    float zBegin;
    float zEnd;

    //voxel attributs
    [Header("Physic simulation")]
    [Tooltip("Select voxel visibility mode, debug is visible, game is invisible")]
    public outputMode VoxelOutputMode;

    [Tooltip("Select a gameObject simulate voxel")]
    public GameObject voxel;
    private GameObject invisibleVoxel;
    private GameObject currentUsingVoxel;
    private List<GameObject> voxels;
    //the pointer of voxels in each frame, to know which voxels we are currently working on
    private int voxelPointer;
    //calculate this once and use it, to gain some performances
    private float voxelY;

    // Use this for initialization
    void Start (){
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

        initializeWebcam();

        webcamFrame = new Color[webCamHeight * webCamWidth];

        //create a new 2d texture to display processed web cam view
        processedWebcamView = new Texture2D(webCamWidth, webCamHeight);

        //display webcam content in this object's material, the content are processed per frame in update methode.
        //rend.material.mainTexture = webcam;
        rend.material.mainTexture = processedWebcamView;

        initializeBackgroundCamera();
        //set background renderer main texture to cam2d
        GameObject backgroundRenderer = GameObject.FindGameObjectWithTag("backgroundRenderer");
        backgroundRenderer.GetComponent<Renderer>().material.mainTexture = cam2d;

        Debug.Log("cam width : " + camWidth);
        Debug.Log("cam height : " + camHeight);
        //initialize the first frame to process state
        imageProcessCycle = imageProcessRate;

        //initialize Game zone
        initializeGameZone();

        //initialize voxel
        voxels = new List<GameObject>();
        voxelY = voxel.transform.localScale.y / 2;
        invisibleVoxel = new GameObject("invisibleVoxel");
        BoxCollider invisibleBox = invisibleVoxel.AddComponent<BoxCollider>() as BoxCollider;
        invisibleVoxel.transform.localScale = voxel.transform.localScale;

        if (VoxelOutputMode == outputMode.debug)
        {
            currentUsingVoxel = voxel;
        }
        else if (VoxelOutputMode == outputMode.game)
        {
            currentUsingVoxel = invisibleVoxel;
        }
    }

    private void initializeGameZone() {
        GameObject murs = GameObject.FindGameObjectsWithTag("backgroundMurs")[0];
        
        for (int i = 0; i < murs.transform.childCount; i++) {
            Transform child = murs.transform.GetChild(i);
            if (child.CompareTag("murOuest")) {
                xBegin = child.position.x;
            }else if (child.CompareTag("murEst")){
                xEnd = child.position.x;
            }else if (child.CompareTag("murSud")){
                zBegin = child.position.z;
            }else if (child.CompareTag("murNord")){
                zEnd = child.position.z;
            }

        }
    }

    private void initializeBackgroundCamera(){
        //get the background camera
        cam = GameObject.FindGameObjectWithTag("backgroundCamera").GetComponent<Camera>();
        camTexture = cam.targetTexture;

        //create pixel array to save main camera (in game) frames
        camHeight = camTexture.height;
        camWidth = camTexture.width;
        cam2d = new Texture2D(camWidth, camHeight, TextureFormat.RGBA32, true);
        camFrame = new Color[camHeight * camWidth];
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
        
        //reset voxel pointer
        voxelPointer = 0;
        //if in game main background camera does exist and be active
        if (cam != null && cam.isActiveAndEnabled) {
            //get webcam pixel array
            webcamFrame = getWebcamPixels();

            //get in game background camera pixel array
            RenderTexture.active = camTexture;
            cam2d.ReadPixels(new Rect(0, 0, camWidth, camHeight), 0, 0);
            cam2d.Apply();
            camFrame = cam2d.GetPixels();

            if (Time.time > nextRefreshTime) {
                nextRefreshTime += backgroundColorRefreshFrequency;
                //process on camera background colors
                //backgroundColors = getBackgroundColors();
                //Debug.Log("there are " + backgroundColors.Count + " differents backgroundColors");  
            }

            if (imageProcessCycle >= imageProcessRate){
                imageProcessCycle = 1;
                //process on webcam frame
                //create a empty temp array
                Color[] tempArray = new Color[webCamWidth*webCamHeight];
                for (int i = 0; i < webcamFrame.Length; i++) {
                    //if is not a background and is a border
                    if (!isRedColor(webcamFrame[i])&& isBorder(webcamFrame, webCamWidth, i)){
                        if (hasBorderAround(tempArray, webCamWidth, i)){
                            //mark this pixel to red in display array
                            webcamFrame[i] = colorTrackingMark;
                            fireVoxel(webCamWidth, webCamHeight, i);
                        }
                        else {
                            //mark this pixel to red in temp array, potential border and noise pixel
                            tempArray[i] = colorTrackingMark;
                        }
                    }
                }
            }
            else {
                imageProcessCycle++;
            }
            cam2d.SetPixels(camFrame);
            cam2d.Apply();
            processedWebcamView.SetPixels(webcamFrame);
            processedWebcamView.Apply();

            //Debug.Log("The number of voxel needed is : " + voxelPointer);
            disableUnusedVoxels(voxelPointer);
        }
        else {
            Debug.LogError("there is no active main  background camera in game, check if your camera is tagged as backgroundCamera");
        }
    }

    private void disableUnusedVoxels(int voxelPointer){
        for (int i = voxelPointer; i < voxels.Count; i++) {
            voxels[i].SetActive(false);
        }
    }

    private bool isRedColor(Color color){
        bool result = false;
        float maxGB = Mathf.Max(color.g, color.b);
        if (color.r > maxGB && maxGB < 100) {
            result = true;
        }
        return result;
    }

    //place voxels from background camera to background(ball in background, foreground are the real objects)
    private void fireVoxel(int webCamWidth, int webCamHeight, int i){
        //calculate each ray's position
        //percentage
        int webcamScreenLigne = i / webCamWidth;
        float xWebcamScreenPercentage = (float)i % webCamWidth / webCamWidth;
        int newI = (int)(webcamScreenLigne * webCamWidth + xWebcamScreenPercentage * camWidth);
        float yWebcamScreenPercentage = (float)webcamScreenLigne / webCamHeight;
        int pixelDisplayPosition = (int)((int)(yWebcamScreenPercentage * camHeight) * camWidth + xWebcamScreenPercentage * camWidth);

        camFrame[pixelDisplayPosition] = colorTrackingMark;

        //fire the voxels with the calculated positions
        float cameraY = cam.transform.position.y;
        // y in camera axis is z in world axis
        float x = xWebcamScreenPercentage;
        float y = yWebcamScreenPercentage;
        if (invertAxisXYFromCamera) {
            x = yWebcamScreenPercentage;
            y = 1 - xWebcamScreenPercentage;
        }
        Vector3 cameraPositionBegin = new Vector3(x, voxelY, y);
        Vector3 worldPosition = CameraPercentPositionToWorldPoint(cameraPositionBegin);

        placeOrInstantiateVoxel(worldPosition);
    }

    private void placeOrInstantiateVoxel(Vector3 worldPosition) {
        //if have instantiated voxels, use them
        if (voxels.Count > voxelPointer){
            GameObject vox = voxels[voxelPointer];
            vox.SetActive(true);
            vox.transform.position = worldPosition;
        }
        else {//if we need more voxels, instanciate them and save them
            GameObject vox = Instantiate(currentUsingVoxel, worldPosition, Quaternion.identity);
            voxels.Add(vox);
        }
        voxelPointer++;
    }

    private Vector3 CameraPercentPositionToWorldPoint(Vector3 cameraPosition) {
        Vector3 result = cameraPosition;
        float availableXScale = xEnd - xBegin;
        float availableZScale = zEnd - zBegin;
        result.x = cameraPosition.x * availableXScale + xBegin;
        result.z = cameraPosition.z * availableZScale + zBegin;
        return result;
    }

    private Color[] getWebcamPixels(){
        Color[] frames = null;
        frames = webcam.GetPixels();
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

    private bool isBorder(Color[] pixelArray, int imageWidth, int pixelPosition) {
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

    private bool hasBorderAround(Color[] pixelArray, int imageWidth, int pixelPosition){
        bool result = false;
        int count = 0;
        //check top and bottom
        for (int i = pixelPosition - imageWidth; i <= pixelPosition + imageWidth; i += imageWidth){
            //check left and right
            for (int j = i - 1; j <= i + 1; j++){
                //begins with j = pixelPosition - imageWidth -1 ends a*on pixelPosition + imageWidth + 1, 9 possible values, the center is self
                if (j > 0 && j < pixelArray.Length && j != pixelPosition && pixelArray[j] == colorTrackingMark){
                    count++;
                    if (count > noiseFilteringLevel) {
                        return true;
                    }     
                }
            }
        }
        return result;
    }
}