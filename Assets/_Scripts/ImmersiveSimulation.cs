using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Drawing;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("Mesh/Dynamic Mesh Generator")]

public class ImmersiveSimulation : MonoBehaviour {

    //MESH DECLARATIONS
   	private List<Vector3> quadVertices = new List<Vector3>();

    [SerializeField]
    private GameObject nodePrefab;
    [SerializeField]
    private GameObject nodeParent;

    private Dictionary<string, CustomPrefab1> nodes = new Dictionary<string, CustomPrefab1>();

    [SerializeField]
    private GameObject controlPointsPrefab;
    [SerializeField]
    private GameObject controlPointParent;

    [SerializeField]
    private Material customShaderMaterial;

    private bool nodeCalculated=false;

	private double meshHeight= 3.0;
	private double maxDisplacement =1.5;


    private Dictionary<string, CustomPrefab1> controlPoints = new Dictionary<string, CustomPrefab1>();

    public class ShellNode
    {
        //public StatNode snode;
		public Vector3 snode;
        public double avgZ;                               //current spatial average
        public double timeAvgZ = 0.0;                       //long exposure average
        public double numSamples;
        public double r, g, b;

        public bool isSupported = false;                     // Initial support conditions  For shape Detection

        //TensorSym stress = new TensorSym();
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

    private int srx = 41;
    private int sry = 41;

    ShellNode[,] n;

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
        setupModel();

        flipMeshNormals(this.gameObject);

        createControlPoints();

		removeExtraControlPoints ();

        renderMeshes();

    }

    // Update is called once per frame
    void Update()
	{
        if (ImmersiveSimulationManager.Instance.changed)
        {
            ImmersiveSimulationManager.Instance.changed=false;

			updateSurrPoints ();

            updateModel();

            renderMeshes();
            
        }

        //take snapshot
        if (Input.GetKeyDown("k"))
        {
            //turnOffControlPoints();

            //takeImageScreenShot();

            //turnOnControlPoints();

            runTensorflow();

            //loadandSaveNewImage();

        }

    }

    #region MeshSetup
    void setupModel()
    {
        n = new ShellNode[srx, sry];
        vizp = new Vector3[srx*sry];
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
                n[i, j].avgZ = 0.0;
                n[i, j].numSamples = 0.0;

				vizp[j * srx + i] = new Vector3((float)x, (float)meshHeight, (float)y);

				vizuv[j * srx + i] = new Vector2( map(3.0f,2.0f,4.0f,0.0f,1.0f), 0.0f);
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
                    viztri[k++] = n0+1+srx;
                    viztri[k++] = n0+1;

                    viztri[k++] = n0;
                    viztri[k++] = n0  +srx;
                    viztri[k++] = n0 + srx+1;
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

    void updateModel()
    {
        int count = 0;
        for (int j = 0; j < sry; ++j)
        {
            for (int i = 0; i < srx; ++i)
            {
				

                GameObject go = GameObject.Find("Control" + count);

				vizuv[count] = new Vector2( map(go.transform.position.y,2.0f,4.0f,0.0f,1.0f), 0.0f);

				//
				n [i, j].snode = new Vector3(go.transform.position.x,go.transform.position.z,go.transform.position.y);

				vizp[count] = new Vector3((float)n[i,j].snode.x, (float)n[i, j].snode.z, (float)n[i, j].snode.y);
                count++;
            }
        }

        count = 0;
		int k = 0;
        for (int j = 0; j < sry - 1; ++j)
        {
            for (int i = 0; i < srx - 1; ++i)
            {
                {
                    //update Mesh vertices
                   	quadVertices[count] = new Vector3((float)n[i, j].snode.x, (float)n[i, j].snode.z, (float)n[i, j].snode.y);
                    count++;
                    quadVertices[count] = (new Vector3((float)n[i + 1, j].snode.x, (float)n[i + 1, j].snode.z, (float)n[i + 1, j].snode.y));
                    count++;
                    quadVertices[count] = (new Vector3((float)n[i + 1, j + 1].snode.x, (float)n[i + 1, j + 1].snode.z, (float)n[i + 1, j + 1].snode.y));
                    count++;
                    quadVertices[count] = (new Vector3((float)n[i, j + 1].snode.x, (float)n[i, j + 1].snode.z, (float)n[i, j + 1].snode.y));
                    count++;

                }
            }
        }

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

		for (int i = 0; i < vertices.Length; i++) {
			colors [i] = UnityEngine.Color.Lerp (UnityEngine.Color.red, UnityEngine.Color.blue, vertices [i].y);
		}
        // assign the array of colors to the Mesh.
        mesh.colors = colors;
    }
    ///////
    #endregion

    #region controlPoints
    void createControlPoints()
    {
        int count = 0;
        for (int j = 0; j < sry; ++j)
        {
            for (int i = 0; i < srx; ++i)
            {
                Vector3 position = new Vector3(n[i, j].snode.x, n[i, j].snode.z, n[i, j].snode.y);
                Vector3 scale = new Vector3(0.05f, 0.05f, 0.05f);

                string tag = "Control";
                createPrefab(controlPoints, count, position, scale, tag, controlPointsPrefab, controlPointParent);
                count++;
            }
        }
    }
	//remove the mesh collider from non draggable ones
	void removeExtraControlPoints()
	{
		int count = 0;
		for (int i = 0; i < srx * sry; i++)
		{
			int iChanged = count / srx;
			int jChanged = count % srx;

			if ((iChanged) %4!=0||(jChanged) %4!=0) {

				GameObject go = GameObject.Find ("Control" + count);
				go.GetComponent<SphereCollider> ().enabled = false;
				go.GetComponent<MeshRenderer> ().enabled = false;
			}

			count++;
		}
	}
    void turnOffControlPoints()
    {
        int count = 0;
        for (int i = 0; i < srx * sry; i++)
        {
            GameObject go = GameObject.Find("Control" + count);
            go.GetComponent<MeshRenderer>().enabled = false;
            count++;
        }
    }

    void turnOnControlPoints()
    {
        int count = 0;
        for (int i = 0; i < srx * sry; i++)
        {
            int iChanged = count / srx;
            int jChanged = count % srx;

            if (!((iChanged) % 4 != 0 || (jChanged) % 4 != 0))
            {

                GameObject go = GameObject.Find("Control" + count);
                go.GetComponent<MeshRenderer>().enabled = true;
            }
            count++;
        }
    }

    void updateSurrPoints()
    {
		GameObject go = GameObject.Find("Control" + ImmersiveSimulationManager.Instance.controlPointChanged);
		Vector3 previousPosition = go.transform.position + new Vector3 (0.0f,ImmersiveSimulationManager.Instance.heightValueChanged,0.0f);
		addDensity(previousPosition, -ImmersiveSimulationManager.Instance.heightValueChanged, 0.8f);
    }

    void addDensity(Vector3 hitPoint, float dy, float a)
    {
        int count = 0;
        for (int i = 0; i < srx * sry; i++)
        {
            int iChanged = count / srx;
            int jChanged = count % srx;

			GameObject go = GameObject.Find ("Control" + count);
            
			if (i != ImmersiveSimulationManager.Instance.controlPointChanged) {
				
				float distance = Vector3.Distance (hitPoint, go.transform.position);
				float yDisplacement = dy * Mathf.Exp (-a * distance * distance);
				go.transform.position = go.transform.position + new Vector3 (0, yDisplacement, 0);
			} 

			//ADD BOUNDS
			//TODO: make them as variables
			if (go.transform.position.y > 4.5f) {

				go.transform.position = new Vector3 (go.transform.position.x,(float)(meshHeight+maxDisplacement),go.transform.position.z);
				
			}else if(go.transform.position.y < 1.5f)
			{
				go.transform.position = new Vector3 (go.transform.position.x,(float)(meshHeight-maxDisplacement),go.transform.position.z);
			}
			//////
	
            count++;
        }
    }

    #endregion

    #region images
    private static string constructPath(string name)
    {
        return string.Format("{0}/tensorflow/{1}",
                             Application.dataPath,name);
    }

    bool takeHiResShot = true;

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
            string filename = constructPath("trial.png");
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            takeHiResShot = true;
        }

    }
    void runTensorflow()
    {
        //TODO: serialize that outside
        string pythonPath = "C:\\Users\\t_abdea\\AppData\\Local\\Programs\\Python\\Python36\\python.exe";

        ////////////run the python tesnorflow///////////////
        string dockerPath = constructPath("tools/dockrun.py");
        dockerPath = dockerPath.Replace('/', '\\');
        string localServerPath = constructPath("server/tools/process-local.py");
        localServerPath = localServerPath.Replace('/', '\\');
        string arguments = "--model_dir \""+ constructPath("model") + "\" --input_file \"" + constructPath("trial.png") + "\"  --output_file \"" + constructPath("output.png")+"\"";
        arguments = arguments.Replace('/', '\\');

        //if (Run)
        {
            Debug.Log(run_cmd(dockerPath, "", pythonPath));
            Debug.Log(run_cmd(localServerPath, arguments, pythonPath));
        }

        ///////////get image from the other folder/////////////
        //System.Drawing.Image output = System.Drawing.Image.FromFile("Z:\\pix2pix-tensorflow\\c_test\\images\\" + name + "-outputs.png");
        Bitmap outputBitmap = new Bitmap(constructPath("output.png"));
    }

    void loadandSaveNewImage()
    {
        Bitmap outputBitmap = new Bitmap(constructPath("trial.png"));

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
                    bmp.SetPixel(i + rx, j, outputBitmap.GetPixel(i,j));
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

    string run_cmd(string cmd, string args, string pythonPath)
    {
        System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo();
        start.FileName = pythonPath;
        start.Arguments = string.Format("\"{0}\" {1}", cmd, args);

        Debug.Log(start.Arguments);

        start.UseShellExecute = false;// Do not use OS shell
        start.CreateNoWindow = true; // We don't need new window
        start.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
        start.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)
        using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                string result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                return result;
            }
        }
    }

    #endregion

}
