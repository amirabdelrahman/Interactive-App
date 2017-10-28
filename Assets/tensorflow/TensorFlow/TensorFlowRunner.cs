using Assets.Scripts.TensorFlow;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;


public class TensorFlowRunner : MonoBehaviour
{


    public string pix2PixLoc;
    private TensorFlowJob tensorFlowJob;

    public GameObject InputSurface;

    
    private static string modelPath;
    private string inputFilePath;

    private OutputMeshGeneration[] meshGenerators;

    void Start()
    {
        meshGenerators = FindObjectsOfType(typeof(OutputMeshGeneration)) as OutputMeshGeneration[];
        modelPath = Path.Combine(pix2PixLoc, "models");
        inputFilePath = Path.Combine(modelPath, "input.png");
    }

    /// <summary>
    /// This functions should be called when the button is pressed.
    /// </summary>
    public void GenerateMaterialDistirbution()
    {
        takeImageScreenShot();
        GenerateMaterialDistribution();
    }

    private void GenerateMaterialDistribution()
    {
        if (tensorFlowJob == null)
        {
            tensorFlowJob = new TensorFlowJob(pix2PixLoc, modelPath, inputFilePath);
            tensorFlowJob.Start(); // Don't touch any data in the job class after you called Start until IsDone is true.
        }
    }

    void Update()
    {
        if (tensorFlowJob != null)
        {
            Debug.Log("TensorFlow is running");
            if (tensorFlowJob.Update())
            {
                // Alternative to the OnFinished callback

                //We can use events and delegates to do it more neatly if we need more sophistitcaed functionality. 
                //InputSurface material.mainTexture = tensorFlowJob.targetMaterialDist;

                for (int i = 0; i < meshGenerators.Length; i++)
                {
                    meshGenerators[i].LoadandSaveNewImage();
                }
                
                tensorFlowJob = null;
            }
        }
    }

    private static string constructPath(string name)
    {
        return string.Format("{0}/tensorflow/{1}",
                             Application.dataPath, name);
    }

    private bool takeHiResShot = true;

    [SerializeField]
    private Camera captureCamera;


    void takeImageScreenShot()
    {
        if (takeHiResShot)
        {
            takeHiResShot = false;
            int resWidth = 256;
            int resHeight = 256;

            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            captureCamera.GetComponent<Camera>().targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            captureCamera.GetComponent<Camera>().Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            captureCamera.GetComponent<Camera>().targetTexture = null;
            RenderTexture.active = null; // added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = constructPath("inputBefore.png");
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            takeHiResShot = true;
            Bitmap outputBitmap = new Bitmap(constructPath("inputBefore.png"));
            int rx = 256;
            int ry = 256;
            Bitmap bmp = new Bitmap(rx, ry);
            Renderer rend = this.GetComponent<Renderer>();
            Texture2D cutoutTexture;

            for (int i = 0; i < rx; i++)
            {
                for (int j = 0; j < ry; j++)
                {
                    bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(127, 127, outputBitmap.GetPixel(i, j).R));
                }
            }
            bmp.Save("C:\\pix2pix-tensorflow\\models\\input.png", System.Drawing.Imaging.ImageFormat.Png);
        }

    }
}






