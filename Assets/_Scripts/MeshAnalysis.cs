using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using millipedeLibNET2;
using millipedeSolversNET;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("Mesh/Dynamic Mesh Generator")]

public class MeshAnalysis : MonoBehaviour {

    //MESH DECLARATIONS
    private List<Vector3> quadVertices = new List<Vector3>();
    [SerializeField]
    private GameObject nodePrefab ;

    private Dictionary<string, CustomPrefab1> nodes = new Dictionary<string, CustomPrefab1>();

    [SerializeField]
    private GameObject optimzedMesh;
    private List<Vector3> OptimizedQuadVertices = new List<Vector3>();
    private Dictionary<string, CustomPrefab1> optimizedNodes = new Dictionary<string, CustomPrefab1>();

    public class ShellNode
    {
        public StatNode snode;
        public double avgZ;                               //current spatial average
        public double timeAvgZ = 0.0;                       //long exposure average
        public double numSamples;
        public double r, g, b;

        public bool isSupported = false;                     // Initial support conditions  For shape Detection

        TensorSym stress = new TensorSym();
        public double vonmises = 0.0;
        public double quadCount = 0.0;
        public bool isPartOfTheModel = true;
    }

    public class ModelParams
    {
        public double a, b, c, d1, d2;
        public double maxd;
        public double maxstress;
    }

    private List<double> vms = new List<double>();

    private int srx = 16;
    private int sry = 16;

    ShellNode[,] n;

    ShellNode[,] nds;

    StatShellQuad[,] q;
    StatShellQuad[,] qO;

    private double x0 = -0.2;
    private double x1 = 0.2;
    private double y0 = -0.2;
    private double y1 = 0.2;

    private double t = 0.0;

    private StatModel Statics;
    private StatModel OptimizedStatics;

    private double minn;
    private double maxx;

    private double minnO;
    private double maxxO;

    // Use this for initialization
    void Start () {

        setupModel();
        

        //mesh

        analyzeModel();
        analyzeOptimized();
        //optimization();

        ApplyMesh(quadVertices, this.gameObject);
        ApplyMesh(OptimizedQuadVertices, optimzedMesh);

        flipMeshNormals(this.gameObject);
        flipMeshNormals(optimzedMesh);

        createNodes();
        renderMeshes();

    }

    // Update is called once per frame
    void Update () {
        animateDeflection();
    }

    void setupModel()
    {
        n = new ShellNode[srx, sry];

        // ^ setting up FE Solver
        //SawapanStatica.StatLicence.Start(0x628364);
        //Statics = new StatSystem();
        Statics = new StatModel();
        StatMaterial m = new StatMaterial(MATERIALTYPES.STEEL, "mat1");
        StatCrossSection sec = new StatCrossSection("sec0", m);
        sec.CircHollow(0.1, 0.11);
        Statics.DeadLoadFactor = 1.0;

        double dx = (x1 - x0) / (srx - 1.0);
        double dy = (y1 - y0) / (sry - 1.0);

        for (int j = 0; j < sry; ++j)
        {
            for (int i = 0; i < srx; ++i)
            {
                double x = x0 + i * dx;
                double y = y0 + j * dy;

                n[i, j] = new ShellNode();
                n[i, j].snode = Statics.AddNode(x, y, 0.7);
                n[i, j].avgZ = 0.0;
                n[i, j].numSamples = 0.0;
                n[i, j].snode.ExtraData = n[i, j];
            }
        }

        // ^ Type all nodes which you want to allocate specific support conditions
        n[0, 0].snode.SupportType = BOUNDARYCONDITIONS.ALL;
        n[srx - 1, sry - 1].snode.SupportType = BOUNDARYCONDITIONS.ALL;
        n[0, sry - 1].snode.SupportType = BOUNDARYCONDITIONS.ALL;
        n[srx - 1, 0].snode.SupportType = BOUNDARYCONDITIONS.ALL;

        q = new StatShellQuad[srx - 1, sry - 1];

        for (int j = 0; j < sry - 1; ++j)
        {
            for (int i = 0; i < srx - 1; ++i)
            {
                {
                    //ADD QUAD TO SOLVER
                    q[i, j] = Statics.AddQuad(n[i, j].snode, n[i + 1, j].snode, n[i + 1, j + 1].snode, n[i, j + 1].snode, m, 0.01);
                    //DRAW THE MESHES
                    quadVertices.Add(new Vector3((float)n[i, j].snode.p.x, (float)n[i, j].snode.p.z, (float)n[i, j].snode.p.y));
                    quadVertices.Add(new Vector3((float)n[i + 1, j].snode.p.x, (float)n[i + 1, j].snode.p.z, (float)n[i + 1, j].snode.p.y));
                    quadVertices.Add(new Vector3((float)n[i + 1, j + 1].snode.p.x, (float)n[i + 1, j + 1].snode.p.z, (float)n[i + 1, j + 1].snode.p.y));
                    quadVertices.Add(new Vector3((float)n[i, j + 1].snode.p.x, (float)n[i, j + 1].snode.p.z, (float)n[i, j + 1].snode.p.y));
                }
            }
        }
        Statics.SolveSystem();
      
    }

    void analyzeModel()
    {
        StatShellQuad q0 = null;
        StatShellQuad q1 = null;
        StatShellQuad q2 = null;
        StatShellQuad q3 = null;

        for (int i = 0; i < srx - 1; i++)
        {
            for (int j = 0; j < sry - 1; j++)
            {
                if (i == 0)
                {
                    if (j == 0)
                    {
                        q0 = q[0, 0];
                        q1 = q[i, 0];
                        q2 = q[i, j];
                        q3 = q[0, j];
                    }
                    else
                    {
                        q0 = q[0, j - 1];
                        q1 = q[i, j - 1];
                        q2 = q[i, j];
                        q3 = q[0, j];
                    }

                }
                else if (j == 0)
                {
                    q0 = q[i - 1, 0];
                    q1 = q[i, 0];
                    q2 = q[i, j];
                    q3 = q[i - 1, j];
                }
                else
                {
                    q0 = q[i - 1, j - 1];
                    q1 = q[i, j - 1];
                    q2 = q[i, j];
                    q3 = q[i - 1, j];
                }
                n[i, j].vonmises = 0.25 * (q0.VonMisesStressMax + q1.VonMisesStressMax + q2.VonMisesStressMax + q3.VonMisesStressMax);
            }
        }

        vms.Clear();

        foreach (StatShellQuad qi in q)
        {
            vms.Add(qi.VonMisesStressMax);
        }

        //calculate minn and maxx
        minn = vms[0];
        maxx = vms[0];
        foreach (double v in vms)
        {
            if (v < minn)
            {
                minn = v;
            }
            if (v > maxx)
            {
                maxx = v;
            }
        }
        ////

    }

    void createNodes()
    {
        int count = 0;
        //Nodes Rendering
        int nodesCount = n.Length;
        foreach (StatNode n in Statics.Nodes)
        {
            Vector3 position = new Vector3((float)n.p.x, (float)n.p.z, (float)n.p.y);
            Vector3 scale = new Vector3(0.002f,0.002f,0.002f);

            string tag = "Nodes";
            createPrefab(nodes, count, position, scale, tag, nodePrefab,this.gameObject);
           
            count++;

        }
        count = 0;
        foreach (StatNode n in OptimizedStatics.Nodes)
        {
            Vector3 position = new Vector3((float)n.p.x, (float)n.p.z, (float)n.p.y);
            Vector3 scale = new Vector3(0.002f, 0.002f, 0.002f);

            string tag = "Nodes";
            createPrefab(optimizedNodes, count + nodesCount, position, scale, tag, nodePrefab, optimzedMesh);
            count++;

        }

    }

    void renderMeshes()
    {
        /////
        //..................................................Quadstrip Visualization
        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        Mesh meshO = optimzedMesh.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] verticesO = meshO.vertices;
        // create new colors array where the colors will be created.
        Color[] colors = new Color[vertices.Length];
        Color[] colorsO = new Color[verticesO.Length];
        
        StatShellQuad q0 = null;
        StatShellQuad q1 = null;
        StatShellQuad q2 = null;
        StatShellQuad q3 = null;

        int count = 0;
        for (int i = 0; i < srx-1; i++)
        {
            for (int j = 0; j < sry-1; j++)
            {
                if (i == 0)
                {
                    if (j == 0)
                    {
                        q0 = q[0, 0];
                        q1 = q[i, 0];
                        q2 = q[i, j];
                        q3 = q[0, j];
                    }
                    else
                    {
                        q0 = q[0, j - 1];
                        q1 = q[i, j - 1];
                        q2 = q[i, j];
                        q3 = q[0, j];
                    }

                }
                else if (j == 0)
                {
                    q0 = q[i - 1, 0];
                    q1 = q[i, 0];
                    q2 = q[i, j];
                    q3 = q[i - 1, j];
                }
                else
                {
                    q0 = q[i - 1, j - 1];
                    q1 = q[i, j - 1];
                    q2 = q[i, j];
                    q3 = q[i - 1, j];
                }

                colors[count] = ColorAt(q0.VonMisesStressMax / maxx);
                count++;
                colors[count] = ColorAt(q3.VonMisesStressMax / maxx);
                count++;
                colors[count] = ColorAt(q2.VonMisesStressMax / maxx);
                count++;
                colors[count] = ColorAt(q1.VonMisesStressMax / maxx);
                count++;
            }
        }

        count = 0;
        for (int i = 0; i < srx - 1; i++)
        {
            for (int j = 0; j < sry - 1; j++)
            {
                if (i == 0)
                {
                    if (j == 0)
                    {
                        q0 = qO[0, 0];
                        q1 = qO[i, 0];
                        q2 = qO[i, j];
                        q3 = qO[0, j];
                    }
                    else
                    {
                        q0 = qO[0, j - 1];
                        q1 = qO[i, j - 1];
                        q2 = qO[i, j];
                        q3 = qO[0, j];
                    }

                }
                else if (j == 0)
                {
                    q0 = qO[i - 1, 0];
                    q1 = qO[i, 0];
                    q2 = qO[i, j];
                    q3 = qO[i - 1, j];
                }
                else
                {
                    q0 = qO[i - 1, j - 1];
                    q1 = qO[i, j - 1];
                    q2 = qO[i, j];
                    q3 = qO[i - 1, j];
                }
                colorsO[count] = ColorAt(q0.VonMisesStressMax / maxxO);
                count++;
                colorsO[count] = ColorAt(q3.VonMisesStressMax / maxxO);
                count++;
                colorsO[count] = ColorAt(q2.VonMisesStressMax / maxxO);
                count++;
                colorsO[count] = ColorAt(q1.VonMisesStressMax / maxxO);
                count++;
            }
        }

        //for (int i = 0; i < vertices.Length; i++)
        //colors[i] = Color.Lerp(Color.red, Color.blue, vertices[i].y);

        // assign the array of colors to the Mesh.
        mesh.colors = colors;
        meshO.colors = colorsO;

    }

    void animateDeflection()
    {
        t += 0.05;
        double du = 0.05 * (1.0 + Math.Cos(t)) / Statics.MaximumDisplacement;
        int count = 0;

        Debug.Log(du);
        foreach (StatNode n in Statics.Nodes)
        {
            //Debug.Log(n.u.z);
            GameObject go = GameObject.Find("Nodes" + count);

            float x = (float)(n.p.x + n.u.x * du);
            float y = (float)(n.p.z + n.u.z * du);
            float z = (float)(n.p.y + n.u.y * du);

            go.transform.position = new Vector3(x,y,z);
            count++;

        }
        count = 0;
        
        du = 0.05 * (1.0 + Math.Cos(t)) / OptimizedStatics.MaximumDisplacement;
        
        foreach (StatNode n in OptimizedStatics.Nodes)
        {
            //Debug.Log(n.u.z);
            int c=count + nds.Length;
            GameObject go = GameObject.Find("Nodes" + c);

            float x = (float)(n.p.x + n.u.x * du);
            float y = (float)(n.p.z + n.u.z * du);
            float z = (float)(n.p.y + n.u.y * du);

            go.transform.position = new Vector3(x, y, z);
            count++;

        }

    }

    public Color ColorAt(double t)
    {
        double r = Math.Sin(t * Math.PI * 0.5);
        double g = 0.4 + Math.Sin(t * Math.PI);
        double b = 0.5 + Math.Cos(t * Math.PI * 0.5);
        
        return new Color((float)r, (float)g, (float)b);
    }

    ////////////////////////////MESH FUNCTIONS
    void ApplyMesh(List<Vector3> vertices, GameObject g)
    {
        Mesh mesh = DynamicMeshGenerator.GenerateMesh(vertices);
        g.GetComponent<MeshFilter>().mesh = mesh;
        //appliedmesh = mesh;
    }
    
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
        Vector3 []
        vertices = mesh.vertices;

        // create new colors array where the colors will be created.
        Color []
        colors = new Color[vertices.Length];
        
        for (int i = 0; i<vertices.Length; i++)
            colors[i] = Color.Lerp(Color.red, Color.blue, vertices[i].y);
        
        // assign the array of colors to the Mesh.
        mesh.colors = colors;
    }
    ///////

    ////Nodes
    void createPrefab(Dictionary<string, CustomPrefab1> prefabs, int c, Vector3 p, Vector3 s, string t, GameObject origin, GameObject parent)
    {
        string prefabName = t + "" + c;
        prefabs.Add(prefabName, new CustomPrefab1(prefabName, p, s, t, origin));
        prefabs[prefabName].Instantiate();
        prefabs[prefabName].access().transform.parent = parent.gameObject.transform;
    }

    void destroyPrefab(Dictionary<string, CustomPrefab1> prefabs, string t)
    {
        for(int i=0;i <prefabs.Count;i++)
        {
            Destroy(GameObject.Find(t+i));
        }
        prefabs.Clear ();
    }

    void optimization ()
    {
        List<ModelParams> results = new List<ModelParams>();

        for (double b = -0.5f; b < 0.5f; b += 0.5f)
        {
            for (double a2 = -0.5f; a2 < 0.5f; a2 += 0.5f)
            {
                for (double c = -1.0f; c < 1.0f; c += 0.5f)
                {
                    for (double d1 = 0.0f; d1 < 3.0f; d1 += 0.5f)
                    {
                        for (double d2 = 0.0f; d2 < 3.0f; d2 += 0.5f)
                        {
                            ModelParams mp = new ModelParams();
                            mp.a = a2;
                            mp.b = b;
                            mp.c = c;
                            mp.d1 = d1;
                            mp.d2 = d2;

                            results.Add(mp);
                        }
                    }
                }
            }
        }
        foreach (ModelParams mpi in results)
        {
            analyzeParamModel(mpi);
        }
        //Parallel.ForEach<ModelParams>(results, AnalyzeParamModel);

        int mini = 0;
        double minimum = 0;

        for (int i = 0; i < results.Count; ++i)
        {
            if (results[i].maxd < results[mini].maxd)
            {
                mini = i;
                minimum = results[mini].maxd;
            }
        }

        Debug.Log(mini+","+ minimum);
        //analyzeOptimized(results[mini]);
    }

    void analyzeParamModel(ModelParams par)
    {
        double a2 = par.a;
        double b = par.b;
        double c = par.c;
        double d1 = par.d1;
        double d2 = par.d2;

        StatModel tempsys = new StatModel();

        StatNode[,] tempnds = new StatNode[srx, sry];

        StatMaterial m = new StatMaterial(MATERIALTYPES.STEEL, "mat1");

        tempsys.DeadLoadFactor = 1.0;

        for (int i = 0; i < srx; i++)
        {
            for (int j = 0; j < sry; j++)
            {
                // displacement for points detected by kinect in order to calculate the optimized form later
                double deformation = a2 * Math.Pow(n[i, j].snode.p.x * n[i, j].snode.p.x, 0.1 * d1) + b * Math.Pow(n[i, j].snode.p.y * n[i, j].snode.p.y, 0.1 * d2) + .1 * c * n[i, j].snode.p.x * n[i, j].snode.p.y;

                //compute new position for points detected by kinect by adding its normal to its iniital position multiplied by the deformation factor
                if (!n[i, j].isSupported)
                {
                    tempnds[i, j] = tempsys.AddNode(n[i, j].snode.p.x, n[i, j].snode.p.y, n[i, j].snode.p.z + deformation);
                }
                else
                {
                    tempnds[i, j] = tempsys.AddNode(n[i, j].snode.p.x, n[i, j].snode.p.y, n[i, j].snode.p.z, BOUNDARYCONDITIONS.ALL);
                }
            }
        }
        
        for (int i = 0; i < srx - 1; i++)
        {
            for (int j = 0; j < sry - 1; j++)
            {
                tempsys.AddQuad(tempnds[i, j], tempnds[i + 1, j], tempnds[i + 1, j + 1], tempnds[i, j + 1], m, 0.01);
            }
        }

        tempsys.SolveSystem();
        par.maxd = tempsys.MaximumDisplacement;
        //Debug.Log(par.a+ ":"+par.maxd);
        //par.maxstress=tempsys.m

    }

    //void analyzeOptimized(ModelParams par)
    void analyzeOptimized()
    {
        double a2 = -0.5;
        double b = -0.5;
        double c = 0;
        double d1 = 2.0;
        double d2 = 2.0;

        Debug.Log("a2 " + a2);
        Debug.Log("b " + b);
        Debug.Log("c " + c);
        Debug.Log("d1 " + d1);
        Debug.Log("d2 " + d2);

        nds = new ShellNode[srx, sry];
        StatNode[,] tempnds = new StatNode[srx, sry];

        StatMaterial m = new StatMaterial(MATERIALTYPES.STEEL, "mat1");

        OptimizedStatics=new StatModel();
        
        OptimizedStatics.DeadLoadFactor = 1.0;
        qO = new StatShellQuad[srx - 1, sry - 1];



        for (int i = 0; i < srx; i++)
        {
            for (int j = 0; j < sry; j++)
            {
                while(n[i,j].snode==null)
                {

                }
                
                // displacement for points detected by kinect in order to calculate the optimized form later
                double deformation = a2 * Math.Pow(n[i, j].snode.p.x * n[i, j].snode.p.x, 0.1 * d1) + b * Math.Pow(n[i, j].snode.p.y * n[i, j].snode.p.y, 0.1 * d2) + .1 * c * n[i, j].snode.p.x * n[i, j].snode.p.y;
                
                nds[i, j] = new ShellNode();
                //compute new position for points detected by kinect by adding its normal to its iniital position multiplied by the deformation factor
                if (!n[i, j].isSupported)
                {

                    nds[i, j].snode = OptimizedStatics.AddNode(n[i, j].snode.p.x, n[i, j].snode.p.y, n[i, j].snode.p.z + deformation);
                    
                }
                else
                {
                    nds[i, j].snode = OptimizedStatics.AddNode(n[i, j].snode.p.x, n[i, j].snode.p.y, n[i, j].snode.p.z, BOUNDARYCONDITIONS.ALL);
                }
            }
        }
        nds[0, 0].snode.SupportType = BOUNDARYCONDITIONS.ALL;
        nds[srx - 1, sry - 1].snode.SupportType = BOUNDARYCONDITIONS.ALL;
        nds[0, sry - 1].snode.SupportType = BOUNDARYCONDITIONS.ALL;
        nds[srx - 1, 0].snode.SupportType = BOUNDARYCONDITIONS.ALL;

        for (int i = 0; i < srx - 1; i++)
        {
            for (int j = 0; j < sry - 1; j++)
            {
                qO[i,j]=OptimizedStatics.AddQuad(nds[i, j].snode, nds[i + 1, j].snode, nds[i + 1, j + 1].snode, nds[i, j + 1].snode, m, 0.01);

                OptimizedQuadVertices.Add(new Vector3((float)nds[i, j].snode.p.x, (float)nds[i, j].snode.p.z, (float)nds[i, j].snode.p.y));
                OptimizedQuadVertices.Add(new Vector3((float)nds[i + 1, j].snode.p.x, (float)nds[i + 1, j].snode.p.z, (float)nds[i + 1, j].snode.p.y));
                OptimizedQuadVertices.Add(new Vector3((float)nds[i + 1, j + 1].snode.p.x, (float)nds[i + 1, j + 1].snode.p.z, (float)nds[i + 1, j + 1].snode.p.y));
                OptimizedQuadVertices.Add(new Vector3((float)nds[i, j + 1].snode.p.x, (float)nds[i, j + 1].snode.p.z, (float)nds[i, j + 1].snode.p.y));

            }
        }

        OptimizedStatics.SolveSystem();
        StatShellQuad q0 = null;
        StatShellQuad q1 = null;
        StatShellQuad q2 = null;
        StatShellQuad q3 = null;

        for (int i = 0; i < srx - 1; i++)
        {
            for (int j = 0; j < sry - 1; j++)
            {
                if (i == 0)
                {
                    if (j == 0)
                    {
                        q0 = qO[0, 0];
                        q1 = qO[i, 0];
                        q2 = qO[i, j];
                        q3 = qO[0, j];
                    }
                    else
                    {
                        q0 = qO[0, j - 1];
                        q1 = qO[i, j - 1];
                        q2 = qO[i, j];
                        q3 = qO[0, j];
                    }

                }
                else if (j == 0)
                {
                    q0 = qO[i - 1, 0];
                    q1 = qO[i, 0];
                    q2 = qO[i, j];
                    q3 = qO[i - 1, j];
                }
                else
                {
                    q0 = qO[i - 1, j - 1];
                    q1 = qO[i, j - 1];
                    q2 = qO[i, j];
                    q3 = qO[i - 1, j];
                }
                nds[i, j].vonmises = 0.25 * (q0.VonMisesStressMax + q1.VonMisesStressMax + q2.VonMisesStressMax + q3.VonMisesStressMax);
            }
        }

        vms.Clear();

        foreach (StatShellQuad qi in qO)
        {
            vms.Add(qi.VonMisesStressMax);
        }

        //calculate minn and maxx
        minnO = vms[0];
        maxxO = vms[0];
        foreach (double v in vms)
        {
            if (v < minnO)
            {
                minnO = v;
            }
            if (v > maxxO)
            {
                maxxO = v;
            }
        }
        ////

    }
}
