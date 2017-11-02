using Assets.Scripts.TensorFlow;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;
using UnityEngine.Video;


public class TensorFlowRunner : MonoBehaviour
{


    public string pix2PixLoc;
    private TensorFlowJob tensorFlowJob;


    private static string modelPath;
    private string inputFilePath;
    public VideoPlayer videoPlayer;

    private ImmersiveSimulation resultMeshRenderer;

    void Start()
    {
        resultMeshRenderer = (ImmersiveSimulation)FindObjectOfType(typeof(ImmersiveSimulation));
        modelPath = Path.Combine(pix2PixLoc, "models");
        inputFilePath = Path.Combine(modelPath, "input.png");
        videoPlayer.Prepare();
    }

    /// <summary>
    /// This functions should be called when the button is pressed.
    /// </summary>
    public void GenerateMaterialDistirbution()
    {
        if (tensorFlowJob == null)
        {
            resultMeshRenderer.TakeImageScreenShot();
            GenerateMaterialDistribution();
        } else {
            Debug.Log("Wait for TensorFlow to finish");
        }
    }

    private void GenerateMaterialDistribution()
    {
        if (tensorFlowJob == null)
        {
            videoPlayer.Play();
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

                resultMeshRenderer.LoadResultMesh();
                tensorFlowJob = null;
                videoPlayer.Stop();
            }
        }
    }

}






