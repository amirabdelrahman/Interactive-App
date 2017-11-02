using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Drawing;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("Mesh/Dynamic Mesh Generator")]

public class ImmersiveSimulation : MonoBehaviour
{


    public struct ShellNode
    {
        public Vector3 p;
        public Vector3 p0;
    }

    private int resolutionX = 41;
    private int resolutionZ = 41;

    ShellNode[,] n;


    private float x0 = -2;
    private float x1 = 2;
    private float z0 = -2;
    private float z1 = 2;

    private float minElevation = -1.0f;
    private float maxElevation = 1.0f;

    private float meshDefautlElevation = 0.0f;
    private float maxDisplacement = 1.5f;

    private float t = 0.0f;

    Mesh srfMesh;
    Vector3[] srfMeshPoints;
    int[] srfMeshTriangles;

    public GameObject InteractionPlane;
    public GameObject DragCursor;

    public GameObject TopLayer;
    public GameObject MidLayer;
    public GameObject BottomLayer;

    public RenderTexture topViewTexture;

    Texture2D layersTexture;

    void LoadOutputImageForLayers(string path)
    {
        if (System.IO.File.Exists(path))
        {
            var bytes = System.IO.File.ReadAllBytes(path);
            layersTexture.LoadImage(bytes);

            layersTexture = ScaleTexture(layersTexture, 32, 32);

            TopLayer.GetComponent<MeshRenderer>().material.mainTexture = layersTexture;
            //MidLayer.GetComponent<MeshRenderer>().material.mainTexture = layersTexture;
            BottomLayer.GetComponent<MeshRenderer>().material.mainTexture = layersTexture;
        }
    }
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);
        for (int i = 0; i < result.height; ++i)
        {
            for (int j = 0; j < result.width; ++j)
            {
                UnityEngine.Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }
        result.Apply();
        return result;
    }
    // Use this for initialization
    void Start()
    {
        layersTexture = new Texture2D(2, 2);
        DragCursor.SetActive(false);
        setupModel();
        //flipMeshNormals(this.gameObject);
        updateMesh();
    }

    bool dragging = false;
    bool tensorFlowIsRunning = false;
    Vector3 dragPointOrigin;
    Vector3 dragPointCursor;

    public void LoadResultMesh()
    {
        LoadOutputImageForLayers(@"C:\pix2pix-tensorflow\models\output.png");
        tensorFlowIsRunning = false;
    }


    void startDragging(RaycastHit hit)
    {
        DragCursor.SetActive(true);
        dragging = true;
        InteractionPlane.transform.position = hit.point;
        InteractionPlane.transform.right = Camera.main.transform.right;
        InteractionPlane.transform.Rotate(Vector3.right, -90.0f);

        dragPointOrigin = hit.point;
        dragPointCursor = dragPointOrigin;

        for (int j = 0; j < resolutionZ; ++j)
        {
            for (int i = 0; i < resolutionX; ++i)
            {
                n[i, j].p0 = n[i, j].p;
            }
        }
    }


    float clampY(float y)
    {
        if (y > maxElevation) return maxElevation;
        if (y < minElevation) return minElevation;
        return y;
    }

    void drag()
    {
        Collider ic = InteractionPlane.GetComponent<Collider>();
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (ic.Raycast(ray, out hit, 2000.0f))
        {
            // dragPointCursor = hit.point;
            dragPointCursor.y = clampY(hit.point.y);
            DragCursor.transform.position = dragPointCursor;
        }

        float dy0 = dragPointCursor.y - dragPointOrigin.y; //displacement added at epicenter of interaction where cursor is
        int k = 0;
        for (int j = 0; j < resolutionZ; ++j)
        {
            for (int i = 0; i < resolutionX; ++i)
            {
                var dp = n[i, j].p0 - dragPointOrigin;
                float d = Mathf.Exp(-0.5f * dp.sqrMagnitude) * dy0;


                n[i, j].p.y = clampY(n[i, j].p0.y + d);

                srfMeshPoints[k++] = n[i, j].p;
            }
        }

        updateMesh();
    }

    void endDragging()
    {
        DragCursor.SetActive(false);
        dragging = false;
        GetComponent<MeshCollider>().sharedMesh = srfMesh;
        InteractionPlane.transform.position = new Vector3(100000.0f, 1000000.0f, 1000000.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!tensorFlowIsRunning)
        {
            if (dragging)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    endDragging();
                }
                else
                {
                    drag();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    MeshCollider mc = GetComponent<MeshCollider>();

                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (mc.Raycast(ray, out hit, 2000.0f))
                    {
                        startDragging(hit);
                    }
                }
            }
        }


    }

    #region MeshSetup
    public void setupModel()
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
                n[i, j].p = new Vector3(x, meshDefautlElevation, z);
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

        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = srfMesh;
    }

    void updateModel()
    {
        int count = 0;
        for (int j = 0; j < resolutionZ; ++j)
        {
            for (int i = 0; i < resolutionX; ++i)
            {

                GameObject go = GameObject.Find("Control" + count);

                //
                n[i, j].p = new Vector3(go.transform.position.x, go.transform.position.z, go.transform.position.y);

                srfMeshPoints[count] = new Vector3((float)n[i, j].p.x, (float)n[i, j].p.z, (float)n[i, j].p.y);
                count++;
            }
        }
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
    private static string constructPath(string name)
    {
        return string.Format("C:\\pix2pix-tensorflow\\models\\{0}", name);
    }

    bool takeHiResShot = true;

    //[SerializeField]
    //private Camera captureCamera;
    public void TakeImageScreenShot()
    {
        tensorFlowIsRunning = true;
        if (takeHiResShot)
        {
            takeHiResShot = false;
            int resWidth = 256;
            int resHeight = 256;

            /* RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
             captureCamera.GetComponent<Camera>().targetTexture = rt;

             Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
             captureCamera.GetComponent<Camera>().Render();
             RenderTexture.active = rt;
             screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
             captureCamera.GetComponent<Camera>().targetTexture = null;
             RenderTexture.active = null; // added to avoid errors
             Destroy(rt);
             */

            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            RenderTexture.active = topViewTexture;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            RenderTexture.active = null; // added to avoid errors



            //byte[] bytes = screenShot.EncodeToPNG();
            //string filename = constructPath("inputBefore.png");

            //if (File.Exists(filename)) File.Delete(filename);


            //File.Create(filename).Dispose();
            //File.WriteAllBytes(filename, bytes);


            //Debug.Log(string.Format("Took screenshot to: {0}", filename));
            takeHiResShot = true;

            //Bitmap outputBitmap = new Bitmap(filename);

            int rx = 256;
            int ry = 256;
            Bitmap bmp = new Bitmap(rx, ry);
            //Renderer rend = this.GetComponent<Renderer>();
            //Texture2D cutoutTexture;

            for (int i = 0; i < rx; i++)
            {
                for (int j = 0; j < ry; j++)
                {
                    //bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(127, 127, outputBitmap.GetPixel(i, j).R));
                    bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(127, 127, (int) map(screenShot.GetPixel(i, j).r, 0.0f ,1.0f ,0f ,255f)));
                }
            }
            bmp.Save(constructPath("input.png"), System.Drawing.Imaging.ImageFormat.Png);

        }

    }

    void LoadandSaveNewImage()
    {
        Bitmap outputBitmap = new Bitmap(constructPath("output.png"));

        int rx = 256;
        int ry = 256;
        ///////////create png (double white) beside it)////////////
        Bitmap bmp = new Bitmap(2 * rx, ry);
        int count = 0;
        for (int i = 0; i < 2 * rx; i++)
        {
            for (int j = 0; j < ry; j++)
            {
                if (i >= rx)
                {
                }
                else
                {
                    //save value displacement bitmap
                    bmp.SetPixel(i + rx, j, outputBitmap.GetPixel(i, j));
                    bmp.SetPixel(i, j, System.Drawing.Color.FromArgb(255, 255, 255));
                    count++;
                }
            }
        }
        bmp.Save(constructPath("trialNew.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
    }

    #endregion

    #region Utilities

    /*public Color ColorAt(double t)
    {
        double r = Math.Sin(t * Math.PI * 0.5);
        double g = 0.4 + Math.Sin(t * Math.PI);
        double b = 0.5 + Math.Cos(t * Math.PI * 0.5);

        return new Color((float)r, (float)g, (float)b);
    }*/

    void createPrefab(Dictionary<string, CustomPrefab1> prefabs, int c, Vector3 p, Vector3 s, string t, GameObject origin, GameObject parent)
    {
        string prefabName = t + "" + c;
        prefabs.Add(prefabName, new CustomPrefab1(prefabName, p, s, t, origin));
        prefabs[prefabName].Instantiate();
        prefabs[prefabName].access().transform.parent = parent.gameObject.transform;
    }

    void destroyPrefab(Dictionary<string, CustomPrefab1> prefabs, string t)
    {
        for (int i = 0; i < prefabs.Count; i++)
        {
            Destroy(GameObject.Find(t + i));
        }
        prefabs.Clear();
    }
    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    #endregion

}
