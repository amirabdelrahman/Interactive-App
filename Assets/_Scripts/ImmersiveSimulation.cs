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

    //private bool nodeCalculated=false;

	


    private Dictionary<string, CustomPrefab1> controlPoints = new Dictionary<string, CustomPrefab1>();

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

    // Use this for initialization
    void Start()
    {
        setupModel();

        flipMeshNormals(this.gameObject);

       // createControlPoints();

		//removeExtraControlPoints ();

        updateMesh();

    }

    bool dragging = false;
    Vector3 dragPointOrigin;
    Vector3 dragPointCursor;

    void startDragging(RaycastHit hit)
    {
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
                float d = Mathf.Exp(-0.5f*dp.sqrMagnitude)*dy0;


                n[i, j].p.y = clampY(n[i, j].p0.y+d);

                srfMeshPoints[k++] = n[i, j].p;
            }
        }

        updateMesh();
    }

    void endDragging()
    {
        dragging = false;
        GetComponent<MeshCollider>().sharedMesh = srfMesh;
        InteractionPlane.transform.position = new Vector3(100000.0f, 1000000.0f, 1000000.0f);
    }

    // Update is called once per frame
    void Update()
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

        if (ImmersiveSimulationManager.Instance.changed)
        {
            ImmersiveSimulationManager.Instance.changed=false;

			updateSurrPoints ();

            updateModel();

            updateMesh();
            
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
        n = new ShellNode[resolutionX, resolutionZ];
        srfMeshPoints = new Vector3[resolutionX*resolutionZ];
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
                    srfMeshTriangles[k++] = n0+1+resolutionX;
                    srfMeshTriangles[k++] = n0+1;

                    srfMeshTriangles[k++] = n0;
                    srfMeshTriangles[k++] = n0  +resolutionX;
                    srfMeshTriangles[k++] = n0 + resolutionX+1;
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
				n [i, j].p = new Vector3(go.transform.position.x,go.transform.position.z,go.transform.position.y);

				srfMeshPoints[count] = new Vector3((float)n[i,j].p.x, (float)n[i, j].p.z, (float)n[i, j].p.y);
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
        for (int j = 0; j < resolutionZ; ++j)
        {
            for (int i = 0; i < resolutionX; ++i)
            {
                Vector3 position = new Vector3(n[i, j].p.x, n[i, j].p.z, n[i, j].p.y);
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
		for (int i = 0; i < resolutionX * resolutionZ; i++)
		{
			int iChanged = count / resolutionX;
			int jChanged = count % resolutionX;

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
        for (int i = 0; i < resolutionX * resolutionZ; i++)
        {
            GameObject go = GameObject.Find("Control" + count);
            go.GetComponent<MeshRenderer>().enabled = false;
            count++;
        }
    }

    void turnOnControlPoints()
    {
        int count = 0;
        for (int i = 0; i < resolutionX * resolutionZ; i++)
        {
            int iChanged = count / resolutionX;
            int jChanged = count % resolutionX;

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
        for (int i = 0; i < resolutionX * resolutionZ; i++)
        {
            int iChanged = count / resolutionX;
            int jChanged = count % resolutionX;

			GameObject go = GameObject.Find ("Control" + count);
            
			if (i != ImmersiveSimulationManager.Instance.controlPointChanged) {
				
				float distance = Vector3.Distance (hitPoint, go.transform.position);
				float yDisplacement = dy * Mathf.Exp (-a * distance * distance);
				go.transform.position = go.transform.position + new Vector3 (0, yDisplacement, 0);
			} 

			//ADD BOUNDS
			//TODO: make them as variables
			if (go.transform.position.y > 4.5f) {

				go.transform.position = new Vector3 (go.transform.position.x,(float)(meshDefautlElevation+maxDisplacement),go.transform.position.z);
				
			}else if(go.transform.position.y < 1.5f)
			{
				go.transform.position = new Vector3 (go.transform.position.x,(float)(meshDefautlElevation-maxDisplacement),go.transform.position.z);
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
       // Bitmap outputBitmap = new Bitmap(constructPath("output.png"));
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
