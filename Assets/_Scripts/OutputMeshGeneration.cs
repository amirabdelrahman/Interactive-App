using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Drawing;


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

   
    #region images
    private static string constructPath( string name)
    {
        return string.Format("{0}\\{1}",
                             "C:\\pix2pix-tensorflow\\models", name);
    }
    

    void loadandSaveNewImage()
    {
        Debug.Log(constructPath("output.png"));
        Bitmap outputBitmap = new Bitmap(constructPath( "output.png"));
        
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
                    bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(outputBitmap.GetPixel(i, j).R, 125, 55, 66));
                }
            }
            bmp.Save(constructPath("outputUp.png"), System.Drawing.Imaging.ImageFormat.Png);
            cutoutTexture = LoadTexture(constructPath("outputUp.png"), rx, ry);
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
                    bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(255- outputBitmap.GetPixel(i, j).R, 255 - outputBitmap.GetPixel(i, j).R, 255 - outputBitmap.GetPixel(i, j).R, 255 - outputBitmap.GetPixel(i, j).R));
                }
            }
            bmp.Save(constructPath( "outputDown.png"), System.Drawing.Imaging.ImageFormat.Png);

            cutoutTexture = LoadTexture(constructPath( "outputDown.png"), rx, ry);
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

    //blur
    private float avgR = 0;
    private float avgG = 0;
    private float avgB = 0;
    private float avgA = 0;
    private float blurPixelCount = 0;
    
    Texture2D FastBlur(Texture2D image, int radius, int iterations)
    {
        Texture2D tex = image;

        for (var i = 0; i < iterations; i++)
        {

            tex = BlurImage(tex, radius, true);
            tex = BlurImage(tex, radius, false);

        }

        return tex;
    }



    Texture2D BlurImage(Texture2D image, int blurSize, bool horizontal)
    {

        Texture2D blurred = new Texture2D(image.width, image.height);
        int _W = image.width;
        int _H = image.height;
        int xx, yy, x, y;

        if (horizontal)
        {
            for (yy = 0; yy < _H; yy++)
            {
                for (xx = 0; xx < _W; xx++)
                {
                    ResetPixel();

                    //Right side of pixel

                    for (x = xx; (x < xx + blurSize && x < _W); x++)
                    {
                        AddPixel(image.GetPixel(x, yy));
                    }

                    //Left side of pixel

                    for (x = xx; (x > xx - blurSize && x > 0); x--)
                    {
                        AddPixel(image.GetPixel(x, yy));

                    }


                    CalcPixel();

                    for (x = xx; x < xx + blurSize && x < _W; x++)
                    {
                        blurred.SetPixel(x, yy, new UnityEngine.Color(avgR, avgG, avgB, 1.0f));

                    }
                }
            }
        }

        else
        {
            for (xx = 0; xx < _W; xx++)
            {
                for (yy = 0; yy < _H; yy++)
                {
                    ResetPixel();

                    //Over pixel

                    for (y = yy; (y < yy + blurSize && y < _H); y++)
                    {
                        AddPixel(image.GetPixel(xx, y));
                    }
                    //Under pixel

                    for (y = yy; (y > yy - blurSize && y > 0); y--)
                    {
                        AddPixel(image.GetPixel(xx, y));
                    }
                    CalcPixel();
                    for (y = yy; y < yy + blurSize && y < _H; y++)
                    {
                        blurred.SetPixel(xx, y, new UnityEngine.Color(avgR, avgG, avgB, 1.0f));

                    }
                }
            }
        }

        blurred.Apply();
        return blurred;
    }
    void AddPixel(UnityEngine.Color pixel)
    {
        avgR += pixel.r;
        avgG += pixel.g;
        avgB += pixel.b;
        blurPixelCount++;
    }

    void ResetPixel()
    {
        avgR = 0.0f;
        avgG = 0.0f;
        avgB = 0.0f;
        blurPixelCount = 0;
    }

    void CalcPixel()
    {
        avgR = avgR / blurPixelCount;
        avgG = avgG / blurPixelCount;
        avgB = avgB / blurPixelCount;
    }
    #endregion

}
