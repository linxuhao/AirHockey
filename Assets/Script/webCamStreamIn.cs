using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class webCamStreamIn : MonoBehaviour
{
    static public webCamStreamIn instance;

    public enum outputMode
    {
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
    [Header("Webcam calibration options %")]
    public int leftOffset;
    public int rightOffset;
    public int topOffset;
    public int botOffset;
    private int usableWebCamHeight;
    private int usableWebCamWidth;
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
    [Tooltip("The gamma mutiplicator to gamma correction on images")]
    public int gammaMultiplicator;

    private Color colorTrackingMark = Color.green;

    HashSet<Color> backgroundColors = new HashSet<Color>();

    ////////////////////////////// Public variables  //////////////////////////////

    [Tooltip("Rate of frame per image process, lower is more accurate but cost in performance. from 1 to infinite....")]
    public int imageProcessRate;
    private int imageProcessCycle;

    [Tooltip("times of blur before processing the image")]
    public int blurStrength;

    //game zone attributs
    [HideInInspector]
    public float xBegin;
    [HideInInspector]
    public float xEnd;
    [HideInInspector]
    public float zBegin;
    [HideInInspector]
    public float zEnd;

    //voxel attributs
    [Header("Physic simulation")]
    [Tooltip("Select voxel visibility mode, debug is visible, game is invisible")]
    public outputMode VoxelOutputMode;

    [Tooltip("Select a gameObject simulate voxel")]
    public GameObject voxel;
    //for game mode, created in script
    private GameObject invisibleVoxel;
    //for mode selection
    private GameObject currentUsingVoxel;
    private List<GameObject> voxels;
    //the pointer of voxels in each frame, to know which voxels we are currently working on
    private int voxelPointer;
    //calculate this once and use it, to gain some performances
    private float voxelY;
    //to put voxels under this game object folder
    private GameObject voxelFolder;
    [Header("Make Player Control Smooth")]
    [Tooltip("From 0 to 1")]
    //value needed by button controllers
    public float currentFrameValuePercentage;
    [Tooltip("The number of frame to memorise data")]
    //value needed by button controllers
    public int frameMemoryNumber ;
    [Tooltip("The number of smooth function applied on raw datas")]
    public int smoothStrengh;

    public bool gameCalibrated = false;
    private bool initialized = false;

    // Use this for initialization
    void Start(){
        if (instance == null)
        {
            instance = this;
        }
    }

    private void init(){
        if (imageProcessRate <= 0)
        {
            Debug.LogError("Please specify imageProcessRate, it is Rate of frame per image process, lower is more accurate but cost in performance.");
        }

        if (grayscaletolerance <= 0)
        {
            Debug.LogError("Please specify grayscaletolerance, it is used to distinguish a border pixel from others.");
        }

        if (webcamResolutionWidth <= 0)
        {
            Debug.LogError("Please specify webcam Resolution Width");
        }

        if (webcamResolutionHeight <= 0)
        {
            Debug.LogError("Please specify webcam Resolution Height");
        }

        rend = GetComponent<Renderer>();

        initializeWebcam();

        webcamFrame = new Color[usableWebCamHeight * usableWebCamWidth];

        //create a new 2d texture to display processed web cam view
        processedWebcamView = new Texture2D(usableWebCamWidth, usableWebCamHeight);

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
        voxelY = 0;
        invisibleVoxel = new GameObject("invisibleVoxel");
        BoxCollider invisibleBox = invisibleVoxel.AddComponent<BoxCollider>() as BoxCollider;
        Rigidbody rb = invisibleVoxel.AddComponent<Rigidbody>() as Rigidbody;
        invisibleVoxel.tag = voxel.tag;
        invisibleVoxel.transform.localScale = voxel.transform.localScale;

        if (VoxelOutputMode == outputMode.debug)
        {
            currentUsingVoxel = voxel;
        }
        else if (VoxelOutputMode == outputMode.game)
        {
            currentUsingVoxel = invisibleVoxel;
        }
        voxelFolder = new GameObject("VoxelObjects");
        voxelFolder.transform.parent = transform;
    }

    private void initializeGameZone()
    {
        GameObject murs = GameObject.FindGameObjectsWithTag("backgroundMurs")[0];
        for (int i = 0; i < murs.transform.childCount; i++)
        {
            Transform child = murs.transform.GetChild(i);
            if (child.CompareTag("murOuest"))
            {
                xBegin = child.position.x;
            }
            else if (child.CompareTag("murEst"))
            {
                xEnd = child.position.x;
            }
            else if (child.CompareTag("murSud"))
            {
                zBegin = child.position.z;
            }
            else if (child.CompareTag("murNord"))
            {
                zEnd = child.position.z;
            }

        }
    }

    private void initializeBackgroundCamera()
    {
        //get the background camera
        cam = GameObject.FindGameObjectWithTag("backgroundCamera").GetComponent<Camera>();
        camTexture = cam.targetTexture;
        if (camTexture) {
            //create pixel array to save main camera (in game) frames
            camHeight = camTexture.height;
            camWidth = camTexture.width;
            cam2d = new Texture2D(camWidth, camHeight, TextureFormat.RGBA32, true);
            camFrame = new Color[camHeight * camWidth];
        }

    }

    private void initializeWebcam()
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
        webcam.requestedHeight = webcamResolutionHeight;
        webcam.requestedWidth = webcamResolutionWidth;
        webcam.Play();

        //create pixel array to save webcam (real world) frames
        webCamHeight = webcam.height;
        webCamWidth = webcam.width;

        usableWebCamHeight = webCamHeight * (100 - topOffset - botOffset) / 100;
        usableWebCamWidth = webCamWidth * (100 - leftOffset - rightOffset) / 100;

        Debug.Log("webcam width : " + webCamWidth + "usable webcam width : " + usableWebCamWidth);
        Debug.Log("webcam height : " + webCamHeight + "usable webcam width : " + usableWebCamHeight);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameCalibrated && !initialized) {
            init();
            initialized = true;
        }
        if (gameCalibrated && initialized) {
            //reset voxel pointer
            voxelPointer = 0;
            //if in game main background camera does exist and be active
            if (cam != null && cam.isActiveAndEnabled)
            {
                //get webcam pixel array
                webcamFrame = getWebcamPixels();
                for (int i = 0; i < blurStrength; i++)
                {
                    webcamFrame = blur(webcamFrame);
                }
                /*
                //get in game background camera pixel array
                RenderTexture.active = camTexture;
                cam2d.ReadPixels(new Rect(0, 0, camWidth, camHeight), 0, 0);
                cam2d.Apply();
                camFrame = cam2d.GetPixels();
                */
                if (imageProcessCycle >= imageProcessRate)
                {
                    imageProcessCycle = 1;
                    //process on webcam frame
                    //create a empty temp array
                    Color[] tempArray = new Color[usableWebCamWidth * usableWebCamHeight];
                    for (int i = 0; i < webcamFrame.Length; i++)
                    {
                        //if is not a background and is a border
                        if (isBorder(webcamFrame, usableWebCamWidth, i))
                        {
                            if (hasBorderAround(tempArray, usableWebCamWidth, i))
                            {
                                //mark this pixel to red in display array
                                webcamFrame[i] = colorTrackingMark;
                                fireVoxel(usableWebCamWidth, usableWebCamHeight, i);
                            }
                            else
                            {
                                //mark this pixel to red in temp array, potential border and noise pixel
                                tempArray[i] = colorTrackingMark;
                            }
                        }
                    }
                }
                else
                {
                    imageProcessCycle++;
                }
                /*cam2d.SetPixels(camFrame);
                cam2d.Apply();*/
                processedWebcamView.SetPixels(webcamFrame);
                processedWebcamView.Apply();

                //Debug.Log("The number of voxel needed is : " + voxelPointer);
                disableUnusedVoxels(voxelPointer);
            }
            else
            {
                Debug.LogError("there is no active main  background camera in game, check if your camera is tagged as backgroundCamera");
            }
        }      
    }

    private void disableUnusedVoxels(int voxelPointer){
        for (int i = voxelPointer; i < voxels.Count; i++){
            voxels[i].SetActive(false);
        }
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
        if (invertAxisXYFromCamera){
            x = yWebcamScreenPercentage;
            y = 1 - xWebcamScreenPercentage;
        }
        Vector3 cameraPositionBegin = new Vector3(x, voxelY, y);
        Vector3 worldPosition = CameraPercentPositionToWorldPoint(cameraPositionBegin);
        placeOrInstantiateVoxel(worldPosition);
    }

    private void placeOrInstantiateVoxel(Vector3 worldPosition)
    {
        //if have instantiated voxels, use them
        if (voxels.Count > voxelPointer)
        {
            GameObject vox = voxels[voxelPointer];
            vox.SetActive(true);
            vox.transform.position = worldPosition;
        }
        else
        {//if we need more voxels, instanciate them and save them
            GameObject vox = Instantiate(currentUsingVoxel, worldPosition, Quaternion.identity);
            //set parent to have a organized inspector
            vox.transform.parent = voxelFolder.transform;
            voxels.Add(vox);
        }
        voxelPointer++;
    }

    private Vector3 CameraPercentPositionToWorldPoint(Vector3 cameraPosition){
        Vector3 result = cameraPosition;
        float availableXScale = xEnd - xBegin;
        float availableZScale = zEnd - zBegin;
        result.x = cameraPosition.x * availableXScale + xBegin;
        result.z = cameraPosition.z * availableZScale + zBegin;
        return result;
    }

    private Color[] getWebcamPixels(){
        Color[] frames = null;
        int xFrom = webCamWidth * leftOffset / 100;
        int yFrom = webCamHeight * topOffset / 100;
        frames = webcam.GetPixels(xFrom, yFrom, usableWebCamWidth, usableWebCamHeight);
        //apply gamma correction on each pixels
        for(int i=0; i< frames.Length; i++) {
            for (int j = 0; j < gammaMultiplicator; j++){
                frames[i] = frames[i].gamma;
            }
        }
        return frames;
    }

    //test if color 1 has big grayscale with color2
    private bool hasBigGrayscaleDifference(Color color1, Color color2, float tolerence){
        Color gamma1 = color1;
        Color gamma2 = color2;
        float grayscale1 = gamma1.grayscale;
        float grayscale2 = gamma2.grayscale;
        float result = Mathf.Abs(grayscale1 - grayscale2);
        //if (count < max && result != 0)
        //{
        //    count++;
        //    Debug.Log("grayscale 1 : " + grayscale1 + ", grayscale 2 : " + grayscale2 + ", the difference is : " + result);
        //}
        return result > tolerence;
    }

    private bool isBorder(Color[] pixelArray, int imageWidth, int pixelPosition)
    {
        bool result = true;
        //check top and bottom
        for (int i = pixelPosition - imageWidth; i <= pixelPosition + imageWidth; i += imageWidth)
        {
            //check left and right
            for (int j = i - 1; j <= i + 1; j++)
            {
                //begins with j = pixelPosition - imageWidth -1 ends a*on pixelPosition + imageWidth + 1, 9 possible values, the center is self
                if (j > 0 && j < pixelArray.Length && j != pixelPosition && !hasBigGrayscaleDifference(pixelArray[j], pixelArray[pixelPosition], grayscaletolerance))
                {
                    // does not have Big Grayscale Difference with nearby pixles means is not a a border
                    return false;
                }
            }
        }
        return result;
    }

    private bool hasBorderAround(Color[] pixelArray, int imageWidth, int pixelPosition){
        int count = 0;
        //check top and bottom
        for (int i = pixelPosition - imageWidth; i <= pixelPosition + imageWidth; i += imageWidth){
            //check left and right
            for (int j = i - 1; j <= i + 1; j++){
                //begins with j = pixelPosition - imageWidth -1 ends a*on pixelPosition + imageWidth + 1, 9 possible values, the center is self
                if (j > 0 && j < pixelArray.Length && j != pixelPosition && pixelArray[j].Equals(colorTrackingMark)){
                    count++;
                    if (count >= noiseFilteringLevel){
                        return true;
                    }
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// blur image for better edge detection, using gaussian filter to supress noise pixels
    /// </summary>
    /// <param name="pixelArray"></param>
    /// <returns></returns>
    private Color[] blur(Color[] pixelArray){

        Color[] result = new Color[pixelArray.Length];

        //gaussian filter
        double[,] filter = new double[3, 3]{
            { 1,2,1 },
            { 2,4,2 },
            { 1,2,1 }
        };

        double factor = 1.0;

        int imageWidth = usableWebCamWidth;

        //for each pixel
        for (int x = 0; x < usableWebCamWidth; x++) {
            for (int y = 0; y < usableWebCamHeight; y++) {
                double red = 0.0, green = 0.0, blue = 0.0;
                int pixelPosition = y * usableWebCamWidth + x;
                //check top and bottom of pixel
                for (int i = 0; i < 3; i++){
                    //check left and right of pixel
                    for (int j = 0; j < 3; j++){
                        int neighbourPosition = (y + i - 1) * usableWebCamWidth + (x + j - 1);
                        //begins with j = pixelPosition - imageWidth -1 ends a*on pixelPosition + imageWidth + 1, 9 possible values, the center is self
                        if (neighbourPosition > 0 && neighbourPosition < pixelArray.Length){
                            Color neighbour = pixelArray[neighbourPosition];
                            red += neighbour.r * filter[i, j] / 16;
                            green += neighbour.g * filter[i, j] / 16;
                            blue += neighbour.b * filter[i, j] / 16;
                        }
                    }
                }
                Color pixel = pixelArray[pixelPosition];
                float r = Math.Min(Math.Max((float)(factor * red), 0), 1);
                float g = Math.Min(Math.Max((float)(factor * green), 0), 1);
                float b = Math.Min(Math.Max((float)(factor * blue), 0), 1);
                result[pixelPosition] = new Color(r, g, b);
            }  
        }
        return result;
    }
}