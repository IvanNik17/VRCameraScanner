namespace OpenCvSharp.Demo
{
	using UnityEngine;
	using UnityEngine.UI;

    using System;

    using System.Collections;
    using OpenCvSharp;

	public class DocumentScannerScript_v2 : MonoBehaviour
	{
		private PaperScanner scanner = new PaperScanner();

        public GameObject visualPlane;

        public GameObject cameraPlane;

        WebCamTexture webcamTexture;

        

        [Range(0.0f, 1.0f)]
        public float noiseReduction_test = 0.8f;

        [Range(0.0f, 1.0f)]
        public float EdgesTight_test = 0.9f;

        [Range(0.0f, 1.0f)]
        public float ExpectedArea_test = 0.2f;

		#region Boring code that combines output image with OpenCV

		/// <summary>
		/// Combines original and processed images into a new twice wide image
		/// </summary>
		/// <param name="original">Source image</param>
		/// <param name="processed">Processed image</param>
		/// <param name="detectedContour">Contour to draw over original image to show detected shape</param>
		/// <returns>OpenCV::Mat image with images combined</returns>
        /// 

        private Mat JustImageContour(Mat original, Point[] detectedContour)
        {
            Size inputSize = new Size(original.Width, original.Height);
            var matContoured = new Mat(new Size(inputSize.Width * 2, inputSize.Height), original.Type(), Scalar.FromRgb(64, 64, 64));
            original.CopyTo(matContoured);
            if (null != detectedContour && detectedContour.Length > 2)
                matContoured.DrawContours(new Point[][] { detectedContour }, 0, Scalar.FromRgb(255, 255, 0), 3);
                
            return matContoured;
        }

		private Mat CombineMats(Mat original, Mat processed, Point[] detectedContour)
		{
			Size inputSize = new Size(original.Width, original.Height);

			// combine fancy output image:
			// - create new texture twice as wide as input
			// - copy input into the left half
			// - draw detected paper contour over original input
			// - put "scanned", un-warped and cleared paper to the right, centered in the right half
			var matCombined = new Mat(new Size(inputSize.Width * 2, inputSize.Height), original.Type(), Scalar.FromRgb(64, 64, 64));

			// copy original image with detected shape drawn over
			original.CopyTo(matCombined.SubMat(0, inputSize.Height, 0, inputSize.Width));
			if (null != detectedContour && detectedContour.Length > 2)
				matCombined.DrawContours(new Point[][] { detectedContour }, 0, Scalar.FromRgb(255, 255, 0), 3);

			// copy scanned paper without extra scaling, as is
			if (null != processed)
			{
				double hw = processed.Width * 0.5, hh = processed.Height * 0.5;
				Point2d center = new Point2d(inputSize.Width + inputSize.Width * 0.5, inputSize.Height * 0.5);
				Mat roi = matCombined.SubMat(
					(int)(center.Y - hh), (int)(center.Y + hh),
					(int)(center.X - hw), (int)(center.X + hw)
				);
				processed.CopyTo(roi);
			}

			return matCombined;
		}

		#endregion

		// Use this for initialization
		public void Process(string name)
		{

            Texture visualTexture;

			var rawImage = gameObject.GetComponent<RawImage>();
			rawImage.texture = null;

			Texture2D inputTexture = (Texture2D)Resources.Load("DocumentScanner/" + name);

			// first of all, we set up scan parameters
			// 
			// scanner.Settings has more values than we use
			// (like Settings.Decolorization that defines
			// whether b&w filter should be applied), but
			// default values are quite fine and some of
			// them are by default in "smart" mode that
			// uses heuristic to find best choice. so,
			// we change only those that matter for us
			scanner.Settings.NoiseReduction = 0.7;											// real-world images are quite noisy, this value proved to be reasonable
			scanner.Settings.EdgesTight = 0.9;												// higher value cuts off "noise" as well, this time smaller and weaker edges
			scanner.Settings.ExpectedArea = 0.2;											// we expect document to be at least 20% of the total image area
			scanner.Settings.GrayMode = PaperScanner.ScannerSettings.ColorMode.Grayscale;	// color -> grayscale conversion mode

			// process input with PaperScanner
			Mat result = null;
			scanner.Input = Unity.TextureToMat(inputTexture);

			// should we fail, there is second try - HSV might help to detect paper by color difference
			if (!scanner.Success)
				// this will drop current result and re-fetch it next time we query for 'Success' flag or actual data
				scanner.Settings.GrayMode = PaperScanner.ScannerSettings.ColorMode.HueGrayscale;

			// now can combine Original/Scanner image
			result = CombineMats(scanner.Input, scanner.Output, scanner.PaperShape);

            Mat resultContoured = null;
            resultContoured = JustImageContour(scanner.Input, scanner.PaperShape);

			// apply result or source (late for a failed scan)
			rawImage.texture = Unity.MatToTexture(result);

            visualTexture = Unity.MatToTexture(resultContoured);

            visualPlane.GetComponent<Renderer>().material.mainTexture = visualTexture;
            //(float)resultContoured.Height / (float)resultContoured.Width
            visualPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1);


			var transform = gameObject.GetComponent<RectTransform>();
			transform.sizeDelta = new Vector2(result.Width, result.Height);
            Debug.Log(resultContoured.Width + "  " + resultContoured.Height);
		}


        public void ProcessCamera()
        {

            Texture visualTexture;

            var rawImage = gameObject.GetComponent<RawImage>();
            rawImage.texture = null;

            Texture2D inputTexture = new Texture2D(webcamTexture.width, webcamTexture.height);

            
            inputTexture.SetPixels(webcamTexture.GetPixels());
            inputTexture.Apply();



            // first of all, we set up scan parameters
            // 
            // scanner.Settings has more values than we use
            // (like Settings.Decolorization that defines
            // whether b&w filter should be applied), but
            // default values are quite fine and some of
            // them are by default in "smart" mode that
            // uses heuristic to find best choice. so,
            // we change only those that matter for us
            scanner.Settings.NoiseReduction = 0.7;											// real-world images are quite noisy, this value proved to be reasonable
            scanner.Settings.EdgesTight = 0.9;												// higher value cuts off "noise" as well, this time smaller and weaker edges
            scanner.Settings.ExpectedArea = 0.2;											// we expect document to be at least 20% of the total image area
            scanner.Settings.GrayMode = PaperScanner.ScannerSettings.ColorMode.Grayscale;	// color -> grayscale conversion mode

            // process input with PaperScanner
            Mat result = null;
            scanner.Input = Unity.TextureToMat(inputTexture);

            // should we fail, there is second try - HSV might help to detect paper by color difference
            if (!scanner.Success)
                // this will drop current result and re-fetch it next time we query for 'Success' flag or actual data
                scanner.Settings.GrayMode = PaperScanner.ScannerSettings.ColorMode.HueGrayscale;

            // now can combine Original/Scanner image
            result = CombineMats(scanner.Input, scanner.Output, scanner.PaperShape);

            Mat resultContoured = null;
            resultContoured = JustImageContour(scanner.Input, scanner.PaperShape);

            // apply result or source (late for a failed scan)
            rawImage.texture = Unity.MatToTexture(result);

            visualTexture = Unity.MatToTexture(resultContoured);

            visualPlane.GetComponent<Renderer>().material.mainTexture = visualTexture;
            //(float)resultContoured.Height / (float)resultContoured.Width
            visualPlane.transform.localScale = new Vector3((float)resultContoured.Width / (float)resultContoured.Height, 1, 1);


            var transform = gameObject.GetComponent<RectTransform>();
            transform.sizeDelta = new Vector2(result.Width, result.Height);
            Debug.Log(resultContoured.Width + "  " + resultContoured.Height);
        }


        //void TestWebCam()
        //{
        //    WebCamDevice[] devices = WebCamTexture.devices;
        //    WebCamTexture webcamTexture = new WebCamTexture();

        //    if (devices.Length > 0)
        //    {
        //        webcamTexture.deviceName = devices[0].name;
        //        Debug.Log(devices[0].name);
        //        Renderer renderer = cameraPlane.GetComponent<Renderer>();
        //        renderer.material.mainTexture = webcamTexture;
        //        webcamTexture.Play();
        //    }
        //}

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

                Renderer renderer = cameraPlane.GetComponent<Renderer>();

                renderer.material.mainTexture = webcamTexture;

            }
        }


		void Start() {
            //Process("Receipt");
            //TestWebCam();
            InitializeCam();
            
		}

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //Renderer renderer = cameraPlane.GetComponent<Renderer>();
                //renderer.material.mainTexture = webcamTexture;


                ProcessCamera();

                //Texture2D inputTexture = new Texture2D(webcamTexture.width, webcamTexture.height);


                //inputTexture.SetPixels(webcamTexture.GetPixels());
                //inputTexture.Apply();

                //Renderer renderer = visualPlane.GetComponent<Renderer>();

                //renderer.material.mainTexture = inputTexture;

                //Process("Receipt");
            }
        }

			
	}
}