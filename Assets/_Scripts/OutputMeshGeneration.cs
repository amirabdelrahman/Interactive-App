using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Drawing;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("Mesh/Dynamic Mesh Generator")]

public class OutputMeshGeneration : MonoBehaviour
{

    public struct ShellNode
    {
        public Vector3 p;
        public Vector3 p0;
    }

    private int resolutionX = 41;
    private int resolutionZ = 41;

    ShellNode[,] n;


    private float x0 = 4;
    private float x1 = 8;
    private float z0 = -2;
    private float z1 = 2;

    [SerializeField]
    private float elevation = 0.0f;

    [SerializeField]
    private int MeshNum = 0;

    private float t = 0.0f;

    Mesh srfMesh;
    Vector3[] srfMeshPoints;
    int[] srfMeshTriangles;
    

    // Use this for initialization
    void Start()
    {
        /*setupModel();
        // flipMeshNormals(this.gameObject);
        updateMesh();*/
    }
    

    // Update is called once per frame
    void Update()
    {
        //take snapshot
        if (Input.GetKeyDown("o"))
        {
            loadandSaveNewImage();

        }

    }

    #region MeshSetup
    void setupModel()
    {
        n = new ShellNode[resolutionX, resolutionZ];

        srfMeshPoints = new Vector3[resolutionX * resolutionZ];
        srfMeshTriangles = new int[(resolutionX - 1) * (resolutionZ - 1) * 6];
        

        float dx = (x1 - x0) / (resolutionX - 1.0f);
        float dy = (z1 - z0) / (resolutionZ - 1.0f);

        for (int j = 0; j < resolutionZ; ++j)
        {
            for (int i = 0; i < resolutionX; ++i)
            {
                float x = x0 + i * dx;
                float z = z0 + j * dy;

                n[i, j] = new ShellNode();
                n[i, j].p = new Vector3(x, elevation, z);
                n[i, j].p0 = n[i, j].p;

                srfMeshPoints[j * resolutionX + i] = n[i, j].p;
                
            }
        }

        int k = 0;
        for (int j = 0; j < resolutionZ - 1; ++j)
        {
            for (int i = 0; i < resolutionX - 1; ++i)
            {
                {
                    int n0 = j * resolutionX + i;


                    srfMeshTriangles[k++] = n0;
                    srfMeshTriangles[k++] = n0 + 1 + resolutionX;
                    srfMeshTriangles[k++] = n0 + 1;

                    srfMeshTriangles[k++] = n0;
                    srfMeshTriangles[k++] = n0 + resolutionX;
                    srfMeshTriangles[k++] = n0 + resolutionX + 1;
                }
            }
        }

        //mesh functions
        srfMesh = new Mesh();
        srfMesh.name = "interactiveMesh";
        srfMesh.vertices = srfMeshPoints;
        srfMesh.triangles = srfMeshTriangles;
        MeshFilter mf = GetComponent<MeshFilter>();
        //  mf.mesh = vizmesh;
        mf.sharedMesh = srfMesh;
        
    }
    

    void updateMesh()
    {
        srfMesh.vertices = srfMeshPoints;

        srfMesh.RecalculateNormals();
        srfMesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = srfMesh;
    }
    #endregion

    #region images
    private static string constructPath(string folder, string name)
    {
        return string.Format("{0}/{2}/{1}",
                             Application.dataPath, name,folder);
    }
    

    void loadandSaveNewImage()
    {
        Bitmap outputBitmap = new Bitmap(constructPath("tensorflow", "output.png"));
        int rx = 256;
        int ry = 256;
        Bitmap bmp = new Bitmap(rx, ry);
        Renderer rend = this.GetComponent<Renderer>();
        Texture2D cutoutTexture;

        //if top
        if (MeshNum == 0)
        {
            for (int i = 0; i < rx; i++)
            {
                for (int j = 0; j < ry; j++)
                {
                    bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(outputBitmap.GetPixel(i, j).R, outputBitmap.GetPixel(i, j).R, outputBitmap.GetPixel(i, j).R, outputBitmap.GetPixel(i, j).R));
                }
            }
            bmp.Save(constructPath("Resources","outputUp.png"), System.Drawing.Imaging.ImageFormat.Png);
            /*string url = "file"+ constructPath("Resources", "outputUp.png");
            WWW www = new WWW(url);
            yield return www;
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.mainTexture = www.texture;*/

            cutoutTexture = LoadTexture(constructPath("Resources", "outputUp.png"), rx, ry);
            cutoutTexture.alphaIsTransparency = true;
            rend.material.mainTexture = cutoutTexture;
            Debug.Log("loaded Texture up");
        }
        //IF BOTTOM LAYER
        else if (MeshNum == 2)
        {
            for (int i = 0; i < rx; i++)
            {
                for (int j = 0; j < ry; j++)
                {
                    //TODO:REVERSE
                    bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(255- outputBitmap.GetPixel(i, j).R, 255 - outputBitmap.GetPixel(i, j).R, 255 - outputBitmap.GetPixel(i, j).R, 255 - outputBitmap.GetPixel(i, j).R));
                }
            }
            bmp.Save(constructPath("Resources", "outputDown.png"), System.Drawing.Imaging.ImageFormat.Png);

            cutoutTexture = LoadTexture(constructPath("Resources", "outputDown.png"), rx, ry);
            cutoutTexture.alphaIsTransparency = true;
            rend.material.mainTexture = cutoutTexture;
            Debug.Log("loaded Texture down");
        }
        
    }

    #endregion

    #region Utilities
    
    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public Texture2D LoadTexture(string filePath, int width, int height)
    {

        Texture2D tex = null;

        try
        {
            var bytes = File.ReadAllBytes(filePath);
            tex = new Texture2D(width, height);
            tex.LoadImage(bytes); //..this will auto-resize the texture dimensions.

        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.ToString());
        }
        return tex;
    }


    #endregion

}
