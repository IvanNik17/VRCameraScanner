using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using OpenCvSharp.Demo;
using Valve.VR;

public class ScanImages_new : MonoBehaviour {


    public SteamVR_Action_Boolean raySelect;

    public SteamVR_Action_Boolean clearSelect;

    public SteamVR_Input_Sources handType_right;



    public GameObject inputPlane;

    //public GameObject outputPlane;

    public GameObject unwrappedPlane;

    public float distTresh = 300f;

    private PaperScanner scanner = new PaperScanner();

    [Range(0.0f, 1.0f)]
    public float noiseReduction_test = 0.7f;

    [Range(0.0f, 1.0f)]
    public float EdgesTight_test = 0.9f;

    [Range(0.0f, 1.0f)]
    public float ExpectedArea_test = 0.2f;

    WebCamTexture webcamTexture;


    //public bool isAutomatic = false;


    //public int operationMode = 0; // 0 - manual, 1 - automatic, 2 - semi-automatic


    public GameObject casterObj_scanner;

    bool isCaptured = false;

    Texture2D savedTexture;

    Texture2D beginningTexture;

    Mat beginingMat;


    Point[] allSelectedPoints;
    int pointCounter = 0;


    Texture2D textureNew;

    bool selectedForChange = false;


    Point closePoint;

    int foundIndex;

    bool mouseMoveHeld = false;

    Vector3 currScale;

    Vector3 currScaleWall;

    bool autoFailed= false;
    


    private int MedianCv(Mat matGray, int max = 256)
    {
        Mat hist = new Mat();
        int[] hdims = { max };
        Rangef[] ranges = { new Rangef(0, max), };
        Cv2.CalcHist(
            new Mat[] { matGray },
            new int[] { 0 },
            null,
            hist,
            1,
            hdims,
            ranges);

        int median = 0, sum = 0;
        int thresh = (int)matGray.Total() / 2;
        while (sum < thresh && median < max)
        {
            sum += (int)(hist.Get<float>(median));
            median++;
        }
        return median;
    }


    private void CalculateThresholdBounds(Mat matGray, out int lower, out int upper, double sigma)
    {
        // prepare
        int median = MedianCv(matGray);
        lower = (int)System.Math.Max(0.0, (1.0 - sigma) * median);
        upper = (int)System.Math.Min(255.0, (1.0 + sigma) * median);
    }


    private Mat JustImageContour(Mat original, Point[] detectedContour)
    {
        Size inputSize = new Size(original.Width, original.Height);
        var matContoured = new Mat(new Size(inputSize.Width * 2, inputSize.Height), original.Type(), Scalar.FromRgb(64, 64, 64));
        original.CopyTo(matContoured);
        if (null != detectedContour && detectedContour.Length > 2)
            matContoured.DrawContours(new Point[][] { detectedContour }, 0, Scalar.FromRgb(255, 255, 0), 3);

        return matContoured;
    }

    private Point[] SortCorners(Point[] corners)
		{
			if (corners.Length != 4)
				throw new OpenCvSharpException("\"corners\" must be an array of 4 elements");

			// divide vertically
			System.Array.Sort<Point>(corners, (a, b) => a.Y.CompareTo(b.Y));
			Point[] tops = new Point[] { corners[0], corners[1] }, bottoms = new Point[] { corners[2], corners[3] };

			// divide horizontally
			System.Array.Sort<Point>(corners, (a, b) => a.X.CompareTo(b.X));
			Point[] lefts = new Point[] { corners[0], corners[1] }, rights = new Point[] { corners[2], corners[3] };

			// fetch final array
			Point[] output = new Point[] {
				tops[0],
				tops[1],
				bottoms[0],
				bottoms[1]
			};
			if (!lefts.Contains(tops[0]))
				output.Swap(0, 1);
			if (!rights.Contains(bottoms[0]))
				output.Swap(2, 3);

			// done
			return output;
		}


	// Use this for initialization
	void Start () {

        //InitializeCam();

        allSelectedPoints = new Point[4];

        //string name = "Form";

        currScale = inputPlane.transform.localScale;

        currScaleWall = unwrappedPlane.transform.localScale;

        //Texture2D inputTexture = (Texture2D)Resources.Load("DocumentScanner/" + name);


        //Texture2D inputTexture = new Texture2D(webcamTexture.width, webcamTexture.height);


        //inputTexture.SetPixels(webcamTexture.GetPixels());
        //inputTexture.Apply();


        //scanner.Settings.NoiseReduction = noiseReduction_test;											// real-world images are quite noisy, this value proved to be reasonable
        //scanner.Settings.EdgesTight = EdgesTight_test;												// higher value cuts off "noise" as well, this time smaller and weaker edges
        //scanner.Settings.ExpectedArea = ExpectedArea_test;	                                        // we expect document to be at least 20% of the total image area

        //scanner.Settings.GrayMode = PaperScanner.ScannerSettings.ColorMode.Grayscale;

        //scanner.Input = OpenCvSharp.Unity.TextureToMat(inputTexture);


        //// should we fail, there is second try - HSV might help to detect paper by color difference
        //if (!scanner.Success)
        //    // this will drop current result and re-fetch it next time we query for 'Success' flag or actual data
        //    scanner.Settings.GrayMode = PaperScanner.ScannerSettings.ColorMode.HueGrayscale;


        // //  ---------------------------------


        //Mat img = OpenCvSharp.Unity.TextureToMat(inputTexture);

        //Mat imgGray = new Mat();

        //Cv2.CvtColor(img,imgGray,ColorConversionCodes.BGR2GRAY);

        //Mat imgGrayBlur = new Mat();

        //Cv2.GaussianBlur(imgGray, imgGrayBlur, new Size(3, 3), 0);


        //int upper, lower;
        //double sigma = 0.33;
        //CalculateThresholdBounds(imgGrayBlur, out lower, out upper, sigma);


        //Mat cannyEdges = new Mat();

        //Cv2.Canny(imgGrayBlur, cannyEdges, 75, 200);


        

        //Point[][] contours;
        //HierarchyIndex[] hierarchyIndexes;
        //Cv2.FindContours(cannyEdges, out contours, out hierarchyIndexes, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);

        //var contourIndex = 0;
        //var previousArea = 0;
        //var biggestContourRect = Cv2.BoundingRect(contours[0]);

        //Point[] bestContour = contours[0];

        //while ((contourIndex >= 0))
        //{
        //    var contour = contours[contourIndex];

        //    var peri = Cv2.ArcLength(contour, true);
        //    var approx = Cv2.ApproxPolyDP(contour, 0.02 * peri, true);

        //    if (approx.Length ==4)
        //    {
        //        var boundingRect = Cv2.BoundingRect(contour); //Find bounding rect for each contour
        //        var boundingRectArea = boundingRect.Width * boundingRect.Height;
        //        if (boundingRectArea > previousArea)
        //        {
        //            biggestContourRect = boundingRect;
        //            previousArea = boundingRectArea;

        //            bestContour = contour;
        //        }
        //    }




        //    contourIndex = hierarchyIndexes[contourIndex].Next;
        //}


        // // ---------------------------------

        //Mat resultContoured = null;

        //resultContoured = JustImageContour(scanner.Input, scanner.PaperShape);




        //Texture2D outputTexture = OpenCvSharp.Unity.MatToTexture(resultContoured);

        //outputPlane.GetComponent<Renderer>().material.mainTexture = outputTexture;
        //outputPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1);

        //inputPlane.GetComponent<Renderer>().material.mainTexture = webcamTexture;
        //inputPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1);



        //Mat resultUnwarpped = null;
        //resultUnwarpped = JustImageContour(scanner.Output, scanner.PaperShape);
        //Texture2D unwrappedTexture = OpenCvSharp.Unity.MatToTexture(resultContoured);

        //unwrappedPlane.GetComponent<Renderer>().material.mainTexture = unwrappedTexture;
        //unwrappedPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1);
        
		
	}

    void InitializeCam()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture(1280, 720, 30);
        if (devices.Length > 0)
        {
            webcamTexture.deviceName = devices[0].name;
            Debug.Log(devices[0].name);
            webcamTexture.Play();



            //Texture2D inputTexture = new Texture2D(webcamTexture.width, webcamTexture.height);

            //IntPtr pointer = webcamTexture.GetNativeTexturePtr();

            //Debug.Log(pointer.ToInt32());
            //inputTexture.UpdateExternalTexture(pointer);

            //Renderer renderer = cameraPlane.GetComponent<Renderer>();

            //renderer.material.mainTexture = webcamTexture;

        }
    }

	
	// Update is called once per frame
	void Update () {

        //string name = "Form";



        //Texture2D inputTexture = (Texture2D)Resources.Load("DocumentScanner/" + name);

        //Texture2D inputTexture = new Texture2D(webcamTexture.width, webcamTexture.height);


        //inputTexture.SetPixels(webcamTexture.GetPixels());
        //inputTexture.Apply();


        scanner.Settings.NoiseReduction = noiseReduction_test;											// real-world images are quite noisy, this value proved to be reasonable
        scanner.Settings.EdgesTight = EdgesTight_test;												// higher value cuts off "noise" as well, this time smaller and weaker edges
        scanner.Settings.ExpectedArea = ExpectedArea_test;	                                        // we expect document to be at least 20% of the total image area

        scanner.Settings.GrayMode = PaperScanner.ScannerSettings.ColorMode.Grayscale;


/*         if (!isCaptured)
        {
            inputPlane.GetComponent<Renderer>().material.mainTexture = webcamTexture;
            inputPlane.transform.localScale = new Vector3((float)webcamTexture.width / (float)webcamTexture.height, 1, 1);
        } */

        //Input.GetKeyDown(KeyCode.Space)
        if (InteractWithScanner.isNewImage && InteractWithScanner.isNewMode)
        {

            //string name = "Form";
            //Texture2D inputTexture = (Texture2D)Resources.Load("DocumentScanner/" + name);
            /* Texture2D inputTexture = new Texture2D(webcamTexture.width, webcamTexture.height);


            inputTexture.SetPixels(webcamTexture.GetPixels());
            inputTexture.Apply();

            inputPlane.GetComponent<Renderer>().material.mainTexture = inputTexture;
            inputPlane.transform.localScale = new Vector3((float)inputTexture.width / (float)inputTexture.height, 1, 1); */


            Texture2D inputTexture = (Texture2D)inputPlane.GetComponent<Renderer>().material.mainTexture;


            if (InteractWithScanner.modeSelected == 0 || InteractWithScanner.modeSelected == 1 || InteractWithScanner.modeSelected == 2)
            {
                isCaptured = true;

                //savedTexture = inputTexture;
                savedTexture = InteractWithScanner.virtualPhoto;
                beginningTexture = InteractWithScanner.virtualPhoto;

                beginingMat = OpenCvSharp.Unity.TextureToMat(beginningTexture);

                allSelectedPoints = new Point[4];

                pointCounter = 0;

                autoFailed = false;
            }

            if (InteractWithScanner.modeSelected == 1 || InteractWithScanner.modeSelected == 2)
            {
                scanner.Input = OpenCvSharp.Unity.TextureToMat(inputTexture);


                // should we fail, there is second try - HSV might help to detect paper by color difference
                // if (!scanner.Success)
                //     // this will drop current result and re-fetch it next time we query for 'Success' flag or actual data
                //     scanner.Settings.GrayMode = PaperScanner.ScannerSettings.ColorMode.HueGrayscale;

                if (!scanner.Success) {
                    Mat failMat = OpenCvSharp.Unity.TextureToMat(inputTexture);
                    var textData = string.Format("{0}", "No contour guess. Use Manual");
                    HersheyFonts textFontFace = HersheyFonts.HersheyPlain;
                    failMat.PutText(textData, new Point(failMat.Width/2, failMat.Height/2), textFontFace, 20, Scalar.White );

                    Texture2D outputTexture = OpenCvSharp.Unity.MatToTexture(failMat);

                    inputPlane.GetComponent<Renderer>().material.mainTexture = outputTexture;

                    //hardcoded values need to be changed
/*                     inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0f, 1f);
                    inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1f, -1f); */

                     inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.25f, 0.75f);
                    inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, -0.5f); 

                    
                    //inputPlane.transform.localScale = new Vector3( currScale.x*((float)resultContoured.Width / (float)resultContoured.Height), currScale.y, currScale.z);

                    textureNew = outputTexture;

                    savedTexture = outputTexture;

                    autoFailed = true;

                    isCaptured = false;
                    
                }
                else{
                    Mat resultContoured = null;



                    for (int i = 0; i < scanner.PaperShape.Length; i++)
                    {
                        float currPointX = (float)scanner.PaperShape[i].X;
                        float currPointY = (float)scanner.PaperShape[i].Y;

                        currPointX /= (float)inputTexture.width;
                        currPointY /= (float)inputTexture.height;

                        currPointX *= (float)inputTexture.width/2f;
                        currPointY *= (float)inputTexture.height/2f;


                        currPointY += ((float)inputTexture.width/2.0f)*0.375f;
                        currPointX += ((float)inputTexture.width/2.0f)*0.5f;

                        scanner.PaperShape[i] = new Point((int)currPointX, (int)currPointY);

                    }
                    




                    resultContoured = JustImageContour(scanner.Input, scanner.PaperShape);

                    allSelectedPoints = scanner.PaperShape;

                    Point2f[] currPointsCon = new Point2f[4];

                    for (int i = 0; i < allSelectedPoints.Length; i++)
                    {
                        resultContoured.DrawMarker(allSelectedPoints[i].X, allSelectedPoints[i].Y, new Scalar(0, 0, 255), MarkerStyle.Cross, 50, LineTypes.Link8, 5);

                        currPointsCon[i] = allSelectedPoints[i];
                    }


                    Texture2D outputTexture = OpenCvSharp.Unity.MatToTexture(resultContoured);

                    inputPlane.GetComponent<Renderer>().material.mainTexture = outputTexture;

                    //hardcoded values need to be changed
                    inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.25f, 0.75f);
                    inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, -0.5f);

                    
                    //inputPlane.transform.localScale = new Vector3( currScale.x*((float)resultContoured.Width / (float)resultContoured.Height), currScale.y, currScale.z);

                    textureNew = outputTexture;

                    /* outputPlane.GetComponent<Renderer>().material.mainTexture = webcamTexture;
                    outputPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1); */



                    Mat test = OpenCvSharp.Demo.MatUtilities.UnwrapShape(beginingMat, currPointsCon);

                    
                    //Mat resultUnwarpped = null;

                    //Point[] noContour = new Point[1];

                    //resultUnwarpped = JustImageContour(scanner.Output, noContour);
                    Texture2D unwrappedTexture = OpenCvSharp.Unity.MatToTexture(test);

                    unwrappedPlane.GetComponent<Renderer>().material.mainTexture = unwrappedTexture;



                    //unwrappedPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1);

                    //isCaptured = true;


                    savedTexture = outputTexture;


                }
                    
                    

                

                

            }




        if (!autoFailed)
        {
            InteractWithScanner.isNewImage = false;
            InteractWithScanner.isNewMode = false;
        }





        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCaptured = false;
        }

        if (isCaptured)
        {
            if (InteractWithScanner.modeSelected == 0 || InteractWithScanner.modeSelected == 1)
            {

                
                RaycastHit hit;
                //var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                

                Mat matCurrent = null;

                if (Physics.Raycast(casterObj_scanner.transform.position, -casterObj_scanner.transform.forward,out hit, 100) && hit.transform.name == "inputPlane")
                {

                    inputPlane.GetComponent<Renderer>().material.mainTexture = savedTexture;

                                    //hardcoded values need to be changed
                    inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.25f, 0.75f);
                    inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, -0.5f);

                    //inputPlane.transform.localScale = new Vector3( currScale.x*((float)savedTexture.width / (float)savedTexture.height), currScale.y, currScale.z);
                    
                    
                    Renderer rend = hit.transform.GetComponent<Renderer>();

                    Texture2D tex = rend.material.mainTexture as Texture2D;
                    Vector2 pixelUV = hit.textureCoord;

                    //pixelUV.x += pixelUV.x*0.75f;
                    

                    pixelUV.x *= tex.width/2;
                    pixelUV.y *= tex.height/2;


                    pixelUV.y += (tex.width/2)*0.375f;
                    pixelUV.x += (tex.width/2)*0.5f;
                    

                    matCurrent = OpenCvSharp.Unity.TextureToMat(tex);
                    Point currPoint = new Point(pixelUV.x, pixelUV.y);


                    if (mouseMoveHeld)
                    {
                        matCurrent.DrawMarker(currPoint.X, currPoint.Y, new Scalar(0, 255, 0), MarkerStyle.Cross, 50, LineTypes.Link8, 5);
                    }
                    else
                    {
                        matCurrent.DrawMarker(currPoint.X, currPoint.Y, new Scalar(255, 0, 0), MarkerStyle.Cross, 50, LineTypes.Link8, 5);
                    }
                   


                    textureNew = OpenCvSharp.Unity.MatToTexture(matCurrent);

                    inputPlane.GetComponent<Renderer>().material.mainTexture = textureNew;
                                    //hardcoded values need to be changed
                    inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.25f, 0.75f);
                    inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, -0.5f);
                    

                    //inputPlane.transform.localScale = new Vector3( currScale.x*((float)textureNew.width / (float)textureNew.height), currScale.y, currScale.z);


                    if (raySelect.GetStateDown(handType_right))
                    {
                        if (InteractWithScanner.modeSelected == 0 )
                        {
                            if (pointCounter <= 4)
                            {

                                matCurrent.DrawMarker(currPoint.X, currPoint.Y, new Scalar(0, 0, 255), MarkerStyle.Cross, 50, LineTypes.Link8, 5);


                                if (pointCounter > 3)
                                {
                                    pointCounter = 0;

                                    allSelectedPoints = new Point[4];

                                    matCurrent = OpenCvSharp.Unity.TextureToMat(beginningTexture);

                                    matCurrent.DrawMarker(currPoint.X, currPoint.Y, new Scalar(0, 0, 255), MarkerStyle.Cross, 50, LineTypes.Link8, 5);




                                }


                                allSelectedPoints[pointCounter] = currPoint;

                                textureNew = OpenCvSharp.Unity.MatToTexture(matCurrent);





                                inputPlane.GetComponent<Renderer>().material.mainTexture = textureNew;
                                                //hardcoded values need to be changed
                                inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.25f, 0.75f);
                                inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, -0.5f);
                                

                                //inputPlane.transform.localScale = new Vector3( currScale.x*((float)textureNew.width / (float)textureNew.height), currScale.y, currScale.z);

                                savedTexture = textureNew;



                            }
                            if (pointCounter == 3)
                            {
                                
                                

                                //Point[] allSelectedPoints_order = new Point[4];

                                allSelectedPoints = SortCorners(allSelectedPoints);



                                Mat resultContoured = null;
                                resultContoured = JustImageContour(matCurrent, allSelectedPoints);

                                Texture2D contourTexture = OpenCvSharp.Unity.MatToTexture(resultContoured);

                                inputPlane.GetComponent<Renderer>().material.mainTexture = contourTexture;
                                                //hardcoded values need to be changed
                                inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.25f, 0.75f);
                                inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, -0.5f);
                                

                                //inputPlane.transform.localScale = new Vector3( currScale.x*((float)resultContoured.Width / (float)resultContoured.Height), currScale.y, currScale.z);


                                savedTexture = contourTexture;

                                Point2f[] currPointsCon = new Point2f[4];

                                for (int i = 0; i < allSelectedPoints.Length; i++)
                                {
                                    currPointsCon[i] = allSelectedPoints[i];
                                }

                                Mat test = OpenCvSharp.Demo.MatUtilities.UnwrapShape(beginingMat, currPointsCon);

                                Texture2D unwrappedTexture = OpenCvSharp.Unity.MatToTexture(test);

                                unwrappedPlane.GetComponent<Renderer>().material.mainTexture = unwrappedTexture;
                                //unwrappedPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1);





                            }


                            pointCounter++;

                        }
                        else if (InteractWithScanner.modeSelected == 1 && !selectedForChange)
                        {
                            closePoint = new Point(-1,-1);
                            foundIndex = -1;

                            // First  selected for now maybe change later
                            for (int i = 0; i < allSelectedPoints.Length; i++)
			                {

                                float distBetweenP = Mathf.Sqrt(Mathf.Pow((currPoint.X - allSelectedPoints[i].X), 2f) + Mathf.Pow((currPoint.Y - allSelectedPoints[i].Y), 2f));
                                if (distBetweenP < distTresh)
                                {
                                    closePoint = allSelectedPoints[i];

                                    foundIndex = i;

                                    selectedForChange = true;

                                    break;
                                }
			                }

                            if (selectedForChange)
                            {

                                inputPlane.GetComponent<Renderer>().material.mainTexture = beginningTexture;
                                                //hardcoded values need to be changed
                                inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.25f, 0.75f);
                                inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, -0.5f);
                                

                                //inputPlane.transform.localScale = new Vector3( currScale.x*((float)beginningTexture.width / (float)beginningTexture.height), currScale.y, currScale.z);

                                Mat currMat = OpenCvSharp.Unity.TextureToMat(beginningTexture);



                                for (int i = 0; i < allSelectedPoints.Length; i++)
                                {
                                    if (i != foundIndex)
                                    {
                                        currMat.DrawMarker(allSelectedPoints[i].X, allSelectedPoints[i].Y, new Scalar(0, 0, 255), MarkerStyle.Cross, 50, LineTypes.Link8, 5);
                                    }
                                    
                                    

                                    
                                }


                                Texture2D changedTexture = OpenCvSharp.Unity.MatToTexture(currMat);

                                inputPlane.GetComponent<Renderer>().material.mainTexture = changedTexture;
                                                //hardcoded values need to be changed
                                inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.25f, 0.75f);
                                inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, -0.5f);
                                

                                //inputPlane.transform.localScale = new Vector3( currScale.x*((float)textureNew.width / (float)textureNew.height), currScale.y, currScale.z);

                                savedTexture = changedTexture;



                            }
                            

                            
                        }

                       
                        


                    }

                    if (raySelect.GetState(handType_right) && selectedForChange)
                    {
                        mouseMoveHeld = true;

                    }
                    
                    if (raySelect.GetStateUp(handType_right) && selectedForChange)
                    {
                        allSelectedPoints[foundIndex] = currPoint;


                        

                        Mat currMat = OpenCvSharp.Unity.TextureToMat(beginningTexture);

                        currMat = JustImageContour(currMat, allSelectedPoints);


                        Point2f[] currPointsCon = new Point2f[4];

                        for (int i = 0; i < allSelectedPoints.Length; i++)
                        {
                            currMat.DrawMarker(allSelectedPoints[i].X, allSelectedPoints[i].Y, new Scalar(0, 0, 255), MarkerStyle.Cross, 50, LineTypes.Link8, 5);

                            currPointsCon[i] = allSelectedPoints[i];

                        }


                        Texture2D changedTexture = OpenCvSharp.Unity.MatToTexture(currMat);

                        inputPlane.GetComponent<Renderer>().material.mainTexture = changedTexture;
                                        //hardcoded values need to be changed
                        inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.25f, 0.75f);
                        inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, -0.5f);
                        

                        //inputPlane.transform.localScale = new Vector3( currScale.x*((float)textureNew.width / (float)textureNew.height), currScale.y, currScale.z);

                        savedTexture = changedTexture;


                        Mat test = OpenCvSharp.Demo.MatUtilities.UnwrapShape(beginingMat, currPointsCon);

                        Texture2D unwrappedTexture = OpenCvSharp.Unity.MatToTexture(test);

                        unwrappedPlane.GetComponent<Renderer>().material.mainTexture = unwrappedTexture;
                        //unwrappedPlane.transform.localScale = new Vector3((float)test.Width / (float)test.Height, 1, 1);




                        mouseMoveHeld = false;

                        selectedForChange = false;
                    }


                }
                
                
                if (clearSelect.GetLastStateDown(handType_right))
                {
                    pointCounter = 0;
                    allSelectedPoints = new Point[4];


                    inputPlane.GetComponent<Renderer>().material.mainTexture = beginningTexture;
                                    //hardcoded values need to be changed
                    inputPlane.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.25f, 0.75f);
                    inputPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f, -0.5f);
                    

                    //inputPlane.transform.localScale = new Vector3( currScale.x*((float)beginningTexture.width / (float)beginningTexture.height), currScale.y, currScale.z);

                    savedTexture = beginningTexture;

                    if (InteractWithScanner.modeSelected == 1)
                    {
                        InteractWithScanner.isNewImage = true;
                        InteractWithScanner.isNewMode = true;

                        isCaptured = false;
                    }

                }





            }




        }


        //if (operationMode == 1)
        //{
        //    scanner.Input = OpenCvSharp.Unity.TextureToMat(webcamTexture);


        //    // should we fail, there is second try - HSV might help to detect paper by color difference
        //    if (!scanner.Success)
        //        // this will drop current result and re-fetch it next time we query for 'Success' flag or actual data
        //        scanner.Settings.GrayMode = PaperScanner.ScannerSettings.ColorMode.HueGrayscale;



        //    Mat resultContoured = null;

        //    resultContoured = JustImageContour(scanner.Input, scanner.PaperShape);




        //    Texture2D outputTexture = OpenCvSharp.Unity.MatToTexture(resultContoured);

        //    outputPlane.GetComponent<Renderer>().material.mainTexture = outputTexture;
        //    outputPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1);

        //    inputPlane.GetComponent<Renderer>().material.mainTexture = webcamTexture;
        //    inputPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1);


        //    Mat resultUnwarpped = null;

        //    Point[] noContour = new Point[1];

        //    resultUnwarpped = JustImageContour(scanner.Output, noContour);
        //    Texture2D unwrappedTexture = OpenCvSharp.Unity.MatToTexture(resultUnwarpped);

        //    unwrappedPlane.GetComponent<Renderer>().material.mainTexture = unwrappedTexture;
        //    unwrappedPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1);
        //}
        //else
        //{

        //    if (!isCaptured)
        //    {
        //        inputPlane.GetComponent<Renderer>().material.mainTexture = webcamTexture;
        //        inputPlane.transform.localScale = new Vector3((float)webcamTexture.width / (float)webcamTexture.height, 1, 1);
        //    }



        //    if (Input.GetKeyDown(KeyCode.Space))
        //    {
                

        //        Texture2D inputTexture = new Texture2D(webcamTexture.width, webcamTexture.height);


        //        inputTexture.SetPixels(webcamTexture.GetPixels());
        //        inputTexture.Apply();

        //        inputPlane.GetComponent<Renderer>().material.mainTexture = inputTexture;
        //        inputPlane.transform.localScale = new Vector3((float)webcamTexture.width / (float)webcamTexture.height, 1, 1);

        //        isCaptured = true;

        //        savedTexture = inputTexture;

        //        beginningTexture = inputTexture;

        //        allSelectedPoints = new Point[4];

        //        pointCounter = 0;

        //    }

        //    if (Input.GetKeyDown(KeyCode.Escape))
        //    {
        //        isCaptured = false;
        //    }


        //    if (isCaptured)
        //    {

                
        //        RaycastHit hit;
        //        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //        Mat matCurrent = null;

        //        if (Physics.Raycast(ray, out hit) && hit.transform.name == "inputPlane")
        //        {

        //            inputPlane.GetComponent<Renderer>().material.mainTexture = savedTexture;
        //            inputPlane.transform.localScale = new Vector3((float)savedTexture.width / (float)savedTexture.height, 1, 1);

        //            Renderer rend = hit.transform.GetComponent<Renderer>();

        //            Texture2D tex = rend.material.mainTexture as Texture2D;
        //            Vector2 pixelUV = hit.textureCoord;
        //            pixelUV.x *= tex.width;
        //            pixelUV.y *= tex.height;

        //            matCurrent = OpenCvSharp.Unity.TextureToMat(tex);
        //            Point currPoint = new Point(pixelUV.x, matCurrent.Height - pixelUV.y);


        //            matCurrent.DrawMarker(currPoint.X, currPoint.Y, new Scalar(255, 0, 0), MarkerStyle.Cross, 100, LineTypes.Link8, 5);


        //            Texture2D textureNew = OpenCvSharp.Unity.MatToTexture(matCurrent);

        //            inputPlane.GetComponent<Renderer>().material.mainTexture = textureNew;
        //            inputPlane.transform.localScale = new Vector3((float)textureNew.width / (float)textureNew.height, 1, 1);


        //            if (Input.GetMouseButtonDown(0))
        //            {


        //                //if (pointCounter > 4)
        //                //{
        //                //    pointCounter = 0;

        //                //    allSelectedPoints = new Point[4];


        //                //    //inputPlane.GetComponent<Renderer>().material.mainTexture = beginningTexture;
        //                //    //inputPlane.transform.localScale = new Vector3((float)beginningTexture.width / (float)beginningTexture.height, 1, 1);



        //                //}


        //                //if (Physics.Raycast(ray, out hit) && hit.transform.name == "inputPlane")
        //                //{



        //                //Debug.Log(pixelUV);

        //                //Mat matCurrent = OpenCvSharp.Unity.TextureToMat(tex);
        //                //Point currPoint = new Point(pixelUV.x, matCurrent.Height - pixelUV.y);











        //                //else if (pointCounter > 3)
        //                //{
        //                //    pointCounter = 0;

        //                //    allSelectedPoints = new Point[4];


        //                //    //inputPlane.GetComponent<Renderer>().material.mainTexture = beginningTexture;
        //                //    //inputPlane.transform.localScale = new Vector3((float)beginningTexture.width / (float)beginningTexture.height, 1, 1);

        //                //    savedTexture = beginningTexture;
        //                //}
        //                if (pointCounter <=4)
        //                {

        //                    matCurrent.DrawMarker(currPoint.X, currPoint.Y, new Scalar(0, 0, 255), MarkerStyle.Cross, 100, LineTypes.Link8, 5);


        //                    if (pointCounter>3)
        //                    {
        //                        pointCounter = 0;

        //                        allSelectedPoints = new Point[4];

        //                        matCurrent = OpenCvSharp.Unity.TextureToMat(beginningTexture);

        //                        matCurrent.DrawMarker(currPoint.X, currPoint.Y, new Scalar(0, 0, 255), MarkerStyle.Cross, 100, LineTypes.Link8, 5);
        //                    }


        //                    allSelectedPoints[pointCounter] = currPoint;

        //                    textureNew = OpenCvSharp.Unity.MatToTexture(matCurrent);



                            

        //                    inputPlane.GetComponent<Renderer>().material.mainTexture = textureNew;
        //                    inputPlane.transform.localScale = new Vector3((float)textureNew.width / (float)textureNew.height, 1, 1);

        //                    savedTexture = textureNew;

                            

        //                }
        //                if (pointCounter == 3)
        //                {
        //                    Debug.Log("HERE");
        //                    Mat resultContoured = null;
        //                    resultContoured = JustImageContour(matCurrent, allSelectedPoints);

        //                    Texture2D contourTexture = OpenCvSharp.Unity.MatToTexture(resultContoured);

        //                    inputPlane.GetComponent<Renderer>().material.mainTexture = contourTexture;
        //                    inputPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1);


        //                    savedTexture = contourTexture;





        //                }
                        

        //                pointCounter++;

        //                //if (pointCounter > 3)
        //                //{
        //                //    //pointCounter = 0;

        //                //    //allSelectedPoints = new Point[4];


        //                //    //inputPlane.GetComponent<Renderer>().material.mainTexture = beginningTexture;
        //                //    //inputPlane.transform.localScale = new Vector3((float)beginningTexture.width / (float)beginningTexture.height, 1, 1);

        //                //   // savedTexture = beginningTexture;
        //                //}



                        

        //                //tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.black);
        //                //tex.Apply();
        //                //}
        //            }


        //        }

        //        if (Input.GetMouseButtonDown(1))
        //        {
        //            pointCounter = 0;
        //            allSelectedPoints = new Point[4];


        //            inputPlane.GetComponent<Renderer>().material.mainTexture = beginningTexture;
        //            inputPlane.transform.localScale = new Vector3((float)beginningTexture.width / (float)beginningTexture.height, 1, 1);

        //        }



                

        //    }


        //}



        
        
	}
}
