# VRCameraScanner
A proof of concept Unity project for using HMD cameras for capturing images from the environment and extracting information. A tool for easily digitalizing real life context for VR presentations/lectures. A user can capture images of documents, black/white boards, etc. from real life using the HMD cameras on the HTC Vive and Valve Index. Then either automatically or manually select the corners of the images and use these points to unwarp the images, as a document scanner. The images are then transformed to a texture and can be displayed on a VR blackboard.


The project is part of the paper - [Testing VR headset cameras for capturing written content](https://dl.acm.org/doi/abs/10.1145/3377290.3377315)

The project consists of two main scripts:
  - InteractWithScanner - contains all the logic for interacting with the scanner interface - the interface is mounted on the left hand and the interactions can be done through raycasting from the right hand
  - ScanImages - interface to the OpenCV+Unity library used for capturing images, cropping, capturing corner points and performing the necessary geometrical image transformations for unwrapping the captured camera images and transforming them to textures.
  
As part of the paper, both the the HTC Vive and the Valve Index cameras were tested and compared to external cameras - a simple webcamera and Intel Realsense camera. This required some changes to the scripts compared to the built-in cameras. The scripts with the changes can be found in "Files for external Cams" folder. The external cameras need to be connected and then the scripts from the folder need to be used instead of the ones in the main Manager object.

# Prerequisites

  - The project requires Unity 2019.3.7f1 or higher to run
  - Additionally, for using OpenCV in Unity, the free library OpenCV+Unity is needed - [OpenCV+Unity](https://assetstore.unity.com/packages/tools/integration/opencv-plus-unity-85928)
  - A HTC Vive or Valve Index is required, plus using the bindings in the project or a webcamera
  
