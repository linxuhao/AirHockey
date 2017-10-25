using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class webCamStreamIn : MonoBehaviour {

    //to display content of webcam on a material
    private Renderer rend;

    //webcam
    private WebCamTexture webcam;
    private int webCamHeight;
    private int webCamWidth;
    private Color32[] webcamFrame;

    //camera
    private Camera cam;
    private RenderTexture camTexture;
    Texture2D cam2d;
    private int camHeight;
    private int camWidth;
    private Color32[] camFrame;

    //new display texture 
    private Texture2D traitedWebcamView;

    //image processing attributs
    //color tolerance
    public int colorTolerance = 20;
    //hashset because it's add and contain's complexity is o(1) and ignore duplicated elements
    private HashSet<Color32> backgroundColors;
    //both in seconds
    private float nextRefreshTime = -0.01f;
    private float refreshFrequency = 1.0f;
   
    // Rate of frame per image process, lower is more accurate but cost in performance.
    public int imageProcessRate;
    private int imageProcessCycle;

    // Use this for initialization
    void Start () {
        if (imageProcessRate <= 0) {
            Debug.LogError("Please specify imageProcessRate, it is Rate of frame per image process, lower is more accurate but cost in performance.");
        }

        if (colorTolerance <= 0)
        {
            Debug.LogError("Please specify colorTolerance, it is the tolerance of color tracking, lower is more accurate but cost in performance and trackability.");
        }

        rend = GetComponent<Renderer>();
        string webCamName = null;

        //find webcams
        WebCamDevice[] devices  = WebCamTexture.devices;
        for (var i = 0; i < devices.Length; i++) {
            //Debug.Log("webcam found : " + devices[i].name);
            webCamName = devices[i].name;
        }

        //initialize the last found webcam(i got only one so) and play
        webcam = new WebCamTexture(webCamName);
        webcam.requestedHeight = 256;
        webcam.requestedWidth = 256;
        webcam.Play();

        //create pixel array to save webcam (real world) frames
        webCamHeight = webcam.height;
        webCamWidth = webcam.width;
        webcamFrame = new Color32[webCamHeight * webCamWidth];
        //create a new 2d texture to display traited web cam view
        traitedWebcamView = new Texture2D(webCamWidth, webCamHeight);

        //display webcam content in this object's material
        //rend.material.mainTexture = webcam;
        rend.material.mainTexture = traitedWebcamView;

        //get main camera into a texture : camTexture
        cam = Camera.main;
        camTexture = cam.targetTexture;

        //create pixel array to save main camera (in game) frames
        camHeight = camTexture.height;
        camWidth = camTexture.width;
        cam2d = new Texture2D(camWidth, camHeight, TextureFormat.RGBA32, true);
        camFrame = new Color32[camHeight * camWidth];

        Debug.Log("webcam width : " + webCamWidth);
        Debug.Log("webcam height : " + webCamHeight);
        Debug.Log("cam width : " + camWidth);
        Debug.Log("cam height : " + camHeight);
        //initialize the first frame to process state
        imageProcessCycle = imageProcessRate;
    }
	
	// Update is called once per frame
	void Update () {
        //if webcam is online and playing, get frames
        if (webcam != null && webcam.isPlaying) {
            //if in game main camera does exist and be active
            if (cam != null && cam.isActiveAndEnabled) {
                //get webcam pixel array
                webcamFrame = webcam.GetPixels32();

                //get in game main camera pixel array
                RenderTexture.active = camTexture;
                cam2d.ReadPixels(new Rect(0, 0, camWidth, camHeight), 0, 0);
                camFrame = cam2d.GetPixels32();

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
                    for (int i = 0; i < webcamFrame.Length; i += 1)
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
                

                traitedWebcamView.SetPixels32(webcamFrame);
                traitedWebcamView.Apply();
            }
            else {
                Debug.LogError("there is no active main camera in game, check if your camera is tagged as MainCamera");
            }
        }
        else {
            Debug.LogError("there is no active webcam");
        }  
    }

    private HashSet<Color32> getBackgroundColors() {
        HashSet<Color32> result = new HashSet<Color32>();
        //process background from camera
        for (int i = 0; i < camFrame.Length; i++) {
            //add operation success only if element not already exist in set
            result.Add(camFrame[i]);
        }
        return result;
    }

    private bool isSameColor(Color32 color1, Color32 color2, int tolerance) {
        bool result = true;
        if (Mathf.Abs(color1.r - color2.r) > tolerance) {
            result = false;
        }else
        if (Mathf.Abs(color1.g - color2.g) > tolerance) {
            result = false;
        }else
        if (Mathf.Abs(color1.b - color2.b) > tolerance) {
            result = false;
        }
        return result;
    }

    private bool isBorder(Color32[] pixelArray, int imageWidth, int imageHeight, int pixelPosition) {
        bool result = true;
        //check top and bottom
        for (int i = pixelPosition - imageWidth; i <= pixelPosition + imageWidth; i += imageWidth) {
            //check left and right
            for (int j = i - 1; j <= i + 1; j++) {
                //begins with j = pixelPosition - imageWidth -1 ends a*on pixelPosition + imageWidth + 1, 9 possible values, the center is self
                
            }
        }
        return result;
    }
}