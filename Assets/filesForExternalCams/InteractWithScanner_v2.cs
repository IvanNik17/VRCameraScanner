using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Valve.VR;
using System.IO;


public class InteractWithScanner_v2 : MonoBehaviour
{

    public SteamVR_Action_Boolean planeShowHide;
    public SteamVR_Action_Boolean raySelect;

    public SteamVR_Action_Boolean captureImage;

    public SteamVR_Input_Sources handType_left;

    public SteamVR_Input_Sources handType_right;

    public GameObject capturePlane;

    public GameObject casterObj;

    public GameObject lineRenderObj;

    public GameObject inputPlane;

    public Material[] ButtonMaterials; 

    public GameObject unwrappedPlane;

    WebCamTexture webcamTexture;

    public static int modeSelected =-1;

    public static bool isNewImage = false;

    public static bool isNewMode = false;

    //public static Vector4 minMaxBounds = new Vector4();
    public static Texture2D virtualPhoto;
    

    Texture2D cameraTexture;

    bool isCamStarted = false;

    Transform objHovered;
    Transform objSelected;

    public RenderTexture processedTexture;

    SteamVR_TrackedCamera.VideoStreamTexture source;

    Vector3 currScale;


    void StartCamera(){

        
        cameraTexture = source.texture;

        if (cameraTexture == null)
        {
            return;
        }

        
        


        capturePlane.GetComponent<Renderer>().material.mainTexture = cameraTexture;

        float aspect = (float)cameraTexture.width / cameraTexture.height;   

        

        VRTextureBounds_t bounds = source.frameBounds;
        capturePlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(bounds.uMin, bounds.vMin);

        float du = bounds.uMax - bounds.uMin;
        float dv = bounds.vMax - bounds.vMin;
        capturePlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(du, dv);

        aspect *= Mathf.Abs(du / dv);

        Vector3 currScaleC = capturePlane.transform.localScale;
        capturePlane.transform.localScale = new Vector3(currScaleC.x, currScaleC.y / aspect, currScaleC.z);

        //processedTexture =new Texture2D(128, 128, TextureFormat.RGBA32, false);
        
        //processedTexture = Texture2D.CreateExternalTexture(capturePlane.GetComponent<Renderer>().material.mainTexture.width,capturePlane.GetComponent<Renderer>().material.mainTexture.height,TextureFormat.ARGB32,false,true,capturePlane.GetComponent<Renderer>().material.mainTexture.GetNativeTexturePtr());
        
       // processedTexture = new Texture2D(cameraTexture.width,cameraTexture.height);
        
        //processedTexture.SetPixels(cameraTexture.GetPixels());
        //processedTexture.Apply(); 


        //Graphics.CopyTexture(cameraTexture, processedTexture);
        
        
        //(Texture2D)capturePlane.GetComponent<Renderer>().material.mainTexture;

        

    }

    void StopCamera(){
        capturePlane.GetComponent<Renderer>().material.mainTexture = null;
        bool undistort = true;
        SteamVR_TrackedCamera.VideoStreamTexture source = SteamVR_TrackedCamera.Source(undistort);
        source.Release();
    }


    void InitializeCam()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture(1920, 1080, 30);
        Debug.Log(devices.Length);
        if (devices.Length > 0)
        {
            webcamTexture.deviceName = devices[2].name;
            Debug.Log(devices[2].name);
            webcamTexture.Play();



            //Texture2D inputTexture = new Texture2D(webcamTexture.width, webcamTexture.height);

            //IntPtr pointer = webcamTexture.GetNativeTexturePtr();

            //Debug.Log(pointer.ToInt32());
            //inputTexture.UpdateExternalTexture(pointer);

            //Renderer renderer = cameraPlane.GetComponent<Renderer>();

            //renderer.material.mainTexture = webcamTexture;

        }
    }

    void startWebCam(){
        capturePlane.GetComponent<Renderer>().material.mainTexture = webcamTexture;

        Vector3 currScaleC = capturePlane.transform.localScale;

        capturePlane.transform.localScale = new Vector3(currScaleC.x, currScaleC.y *( currScaleC.y / currScaleC.x), currScaleC.z);
        
        
        
    }

    // Start is called before the first frame update
    void Start()
    {
        LineRenderer lineRenderer = lineRenderObj.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.positionCount = 2;

        objHovered = null;
        objSelected = null;

        InitializeCam();

        currScale = inputPlane.transform.localScale;
        currScale.x *= (float)webcamTexture.width/(float)webcamTexture.height;

        inputPlane.transform.localScale = currScale;

        /* bool undistort = true;
        source = SteamVR_TrackedCamera.Source(undistort);
        
        source.Acquire(); */
        

        /* currScale = inputPlane.transform.localScale;
        currScale.x *= (float)source.texture.width/(float)source.texture.height;

        inputPlane.transform.localScale = currScale; */
        //capturePlane.transform.localScale = new Vector3((float)cameraTexture.width / (float)cameraTexture.height, 1, 1);



        /* planeShowHide.AddOnStateUpListener(TriggerShow,handType);
        planeShowHide.AddOnStateDownListener(TriggerHide,handType); */

/*         WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture();

        
        
        if (devices.Length > 0)
        {
            webcamTexture.deviceName = devices[0].name;
            Debug.Log(devices[0].name);
            webcamTexture.Play();

            capturePlane.GetComponent<Renderer>().material.mainTexture = webcamTexture;
            //capturePlane.transform.localScale = new Vector3((float)webcamTexture.width / (float)webcamTexture.height, 1, 1);
        } */

    }

    // Update is called once per frame
    void Update()
    {
        if (planeShowHide.GetState(handType_left))
        {
            if (!isCamStarted)
            {
                //StartCamera();
                startWebCam();
                isCamStarted = true;
            }

            capturePlane.GetComponent<MeshRenderer>().enabled = true;
        }
        else{
            capturePlane.GetComponent<MeshRenderer>().enabled = false;
            if (isCamStarted)
            {
                isCamStarted = false;
               // StopCamera();
            }
        }


        ButtonInteract();

        if (captureImage.GetStateDown(handType_left) && isCamStarted)
        {
            processedTexture = new RenderTexture(webcamTexture.width,webcamTexture.height, 16, RenderTextureFormat.ARGB32);
            Graphics.Blit(webcamTexture,processedTexture);
            RenderTexture.active = processedTexture;

            virtualPhoto = new Texture2D(webcamTexture.width,webcamTexture.height, TextureFormat.RGB24, false);
            
            virtualPhoto.ReadPixels( new Rect(0, 0, webcamTexture.width,webcamTexture.height), 0, 0);

            virtualPhoto.Apply(); 


            inputPlane.GetComponent<Renderer>().material.mainTexture = virtualPhoto;

/*             VRTextureBounds_t bounds = source.frameBounds;

            

            inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(bounds.uMin, bounds.vMin);

            float du = bounds.uMax - bounds.uMin;
            float dv = bounds.vMax - bounds.vMin; 
            inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(du, dv);

            Debug.Log(bounds.uMin + " | " + bounds.vMin + " | " + bounds.uMax + " | " + bounds.vMax + " | ");
            Debug.Log(du + " | " + dv); */
            
            //Vector3 currScale = inputPlane.transform.localScale;

            //inputPlane.transform.localScale = new Vector3( currScale.x*((float)virtualPhoto.width / (float)virtualPhoto.height), currScale.y, currScale.z);
            if (modeSelected != -1)
            {
               isNewImage = true;
               isNewMode = true; 
            }
            else{
               isNewImage = true;
            }
            
            
            // inputTexture.SetPixels(currCapture.GetPixels());
            // inputTexture.Apply(); 
        }

    }


    void ButtonInteract(){
        RaycastHit hit;

        LineRenderer lineRenderer = lineRenderObj.GetComponent<LineRenderer>();
        
        Vector3 endPos = casterObj.transform.position - casterObj.transform.forward;
        Debug.DrawRay(casterObj.transform.position, -casterObj.transform.forward, Color.red);
        if(Physics.Raycast(casterObj.transform.position, -casterObj.transform.forward,out hit, 100) && hit.transform.tag == "interButton"){
            
            objHovered = hit.transform;
            Debug.Log(objHovered.name);
            if (objSelected == null || (hit.transform !=objSelected) )
            {
                objHovered.GetComponent<Renderer>().material = ButtonMaterials[1];
            }
            
            endPos = hit.point;

            if (raySelect.GetStateDown(handType_right))
            {
                
                if (objSelected != null)
                {
                    objSelected.GetComponent<Renderer>().material = ButtonMaterials[0];
                }
                objSelected = hit.transform;
                objSelected.GetComponent<Renderer>().material = ButtonMaterials[2];

                modeSelected = WhichButton(objSelected);
                

                if (inputPlane.GetComponent<Renderer>().material.mainTexture != null)
                {
                    isNewMode = true;
                    isNewImage = true;

                    inputPlane.GetComponent<Renderer>().material.mainTexture = virtualPhoto;
                }
                else{
                    isNewMode = true;
                }
                

                Debug.Log(modeSelected);
            }

        } 
        else {
            if (objHovered != null &&  objHovered != objSelected)
            {
                objHovered.GetComponent<Renderer>().material = ButtonMaterials[0];
            }
            
        }

        

        lineRenderer.SetPosition(0,casterObj.transform.position);
        lineRenderer.SetPosition(1,endPos);

        if (Input.GetKeyDown(KeyCode.S))
        {
            Texture2D outputTexture = (Texture2D)unwrappedPlane.GetComponent<Renderer>().material.mainTexture;

            byte[] bytes = outputTexture.EncodeToPNG();

            File.WriteAllBytes(Application.dataPath + "/../outputUnwrapped.png", bytes);

            Debug.Log("Saved");
        }

    }

    int WhichButton(Transform selectedObj){

        int buttonNum=-1;
        switch (selectedObj.name)
        {
            case "buttonManual":
                buttonNum = 0;
                break;
            case "buttonSemi":
                buttonNum = 1;
                break;

            case "buttonAuto":
                buttonNum = 2;
                break;

            default:
                buttonNum = -1;
                break;
        }

        return buttonNum;
    }

/*     public void TriggerShow(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("HERE 1");
        inputPlane.GetComponent<MeshRenderer>().enabled = false;
    }
    public void TriggerHide(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("HERE 2");
        inputPlane.GetComponent<MeshRenderer>().enabled = true;
    } */




}
