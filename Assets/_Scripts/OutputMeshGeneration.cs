using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Drawing;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("Mesh/Dynamic Mesh Generator")]

public class OutputMeshGeneration : MonoBehaviour {

    //MESH DECLARATIONS
    private List<Vector3> quadVertices = new List<Vector3>();
    private List<Vector3> quadVerticesUo = new List<Vector3>();
    private List<Vector3> quadVerticesDown = new List<Vector3>();

    [SerializeField]
    private Material customShaderMaterial;
    

    private double meshHeight = 3.0;
    private double maxDisplacement = 1.5;
    

    public class ShellNode
    {
        //public StatNode snode;
        public Vector3 snode;
        public double r, g, b;
        
        
    }


    private List<double> vms = new List<double>();

    private int srx = 16;
    private int sry = 16;

    ShellNode[,] n;
    ShellNode[,] nUp;
    ShellNode[,] nDown;

    ShellNode[,] nds;

    //StatShellQuad[,] q;

    private double x0 = -2;
    private double x1 = 2;
    private double y0 = -2;
    private double y1 = 2;

    private double t = 0.0;

    //private StatModel Statics;

    private double minn;
    private double maxx;

    Mesh vizmesh;
    Vector3[] vizp;
    Vector2[] vizuv; // vm , thickness
    Vector2[] vizuv2; //P1, P2
    Vector2[] vizuv3; //Bm, Ux
    Vector2[] vizuv4; //Uy, Uz
    int[] viztri;

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
            //Bitmap outputBitmap = new Bitmap(constructPath("output.png"));
            //outputBitmap.SetPixel(i + rx, j, outputBitmap.GetPixel(i, j));

            setupModel();

            flipMeshNormals(this.gameObject);

            renderMeshes();

        }

    }

    #region MeshSetup
    void setupModel()
    {
        n = new ShellNode[srx, sry];
        vizp = new Vector3[srx * sry];
        vizuv = new Vector2[srx * sry];
        vizuv2 = new Vector2[srx * sry];
        vizuv3 = new Vector2[srx * sry];
        vizuv4 = new Vector2[srx * sry];
        viztri = new int[(srx - 1) * (sry - 1) * 6];

        double dx = (x1 - x0) / (srx - 1.0);
        double dy = (y1 - y0) / (sry - 1.0);

        for (int j = 0; j < sry; ++j)
        {
            for (int i = 0; i < srx; ++i)
            {
                double x = x0 + i * dx;
                double y = y0 + j * dy;

                n[i, j] = new ShellNode();
                n[i, j].snode = new Vector3((float)x, (float)y, (float)meshHeight);

                vizp[j * srx + i] = new Vector3((float)x, (float)meshHeight, (float)y);

                vizuv[j * srx + i] = new Vector2(map(3.0f, 2.0f, 4.0f, 0.0f, 1.0f), 0.0f);
            }
        }

        int k = 0;
        for (int j = 0; j < sry - 1; ++j)
        {
            for (int i = 0; i < srx - 1; ++i)
            {
                {
                    int n0 = j * srx + i;

                    //DRAW THE MESHES
                    quadVertices.Add(new Vector3((float)n[i, j].snode.x, (float)n[i, j].snode.z, (float)n[i, j].snode.y));
                    quadVertices.Add(new Vector3((float)n[i + 1, j].snode.x, (float)n[i + 1, j].snode.z, (float)n[i + 1, j].snode.y));
                    quadVertices.Add(new Vector3((float)n[i + 1, j + 1].snode.x, (float)n[i + 1, j + 1].snode.z, (float)n[i + 1, j + 1].snode.y));
                    quadVertices.Add(new Vector3((float)n[i, j + 1].snode.x, (float)n[i, j + 1].snode.z, (float)n[i, j + 1].snode.y));

                    viztri[k++] = n0;
                    viztri[k++] = n0 + 1 + srx;
                    viztri[k++] = n0 + 1;

                    viztri[k++] = n0;
                    viztri[k++] = n0 + srx;
                    viztri[k++] = n0 + srx + 1;
                }
            }
        }

        //mesh functions
        vizmesh = new Mesh();
        vizmesh.vertices = vizp;
        vizmesh.triangles = viztri;
        vizmesh.uv = vizuv;
        vizmesh.uv2 = vizuv2;
        vizmesh.uv3 = vizuv3;
        vizmesh.uv4 = vizuv4;
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh = vizmesh;

    }
    
    void renderMeshes()
    {
        vizmesh.vertices = vizp;
        vizmesh.uv = vizuv;
        vizmesh.uv2 = vizuv3;
        vizmesh.uv3 = vizuv3;
        vizmesh.uv4 = vizuv4;

        vizmesh.RecalculateNormals();
        vizmesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = vizmesh;
        Debug.Log("Mesh reassigned");
    }
    #endregion


    #region MeshFunctions
    void flipMeshNormals(GameObject g)
    {
        MeshFilter filter = g.GetComponent(typeof(MeshFilter)) as MeshFilter;
        if (filter != null)
        {
            Mesh mesh = filter.mesh;

            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                int[] triangles = mesh.GetTriangles(m);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                mesh.SetTriangles(triangles, m);
            }
        }
    }

    void changeVertixMeshColor()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        // create new colors array where the colors will be created.
        UnityEngine.Color[] colors = new UnityEngine.Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = UnityEngine.Color.Lerp(UnityEngine.Color.red, UnityEngine.Color.blue, vertices[i].y);
        }
        // assign the array of colors to the Mesh.
        mesh.colors = colors;
    }
    ///////
    #endregion

    #region Utilities

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    private static string constructPath(string name)
    {
        return string.Format("{0}/tensorflow/{1}",
                             Application.dataPath, name);
    }

    #endregion

}
