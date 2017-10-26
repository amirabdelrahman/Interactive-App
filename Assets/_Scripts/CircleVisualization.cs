using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CircleVisualization : MonoBehaviour {

	public UtilMove _UtilMove;

	private double height=0; 
	private double width=0;


	//spacing
	private double du=0;
	private double dv=0;

	//object Scale
	private Vector3 planeScale=new Vector3();

	//object position
	private Vector3 planePosition=new Vector3();

	//object rotation
	private Quaternion planeRotation=new Quaternion();

	//[SerializeField]
	//private GameObject dummy;
	//private CustomPrefab1 d;
	//private CustomPrefab1 d1;
	//[SerializeField]
	//private GameObject dummy1;

	[SerializeField]
	private int code=1000;

	//number of circles in x and y

	private int nu=10;
	private int nv=10;

	private int counter=0;

	private Dictionary<string, CustomPrefab1> forgroundCircles = new Dictionary<string, CustomPrefab1>();
	private Dictionary<string, CustomPrefab1> backgroundCircles = new Dictionary<string, CustomPrefab1>();
	private Dictionary<string, CustomPrefab1> suggestion = new Dictionary<string, CustomPrefab1>();

	// Use this for initialization
	void Start () {
		
		planeScale = this.gameObject.transform.localScale;
		planePosition = this.gameObject.transform.localPosition;
		planeRotation = gameObject.transform.rotation;

		Debug.Log (code + " rotation:" + planeRotation);
		height = planeScale.x * 10.0;
		width = planeScale.z * 10.0;
		Debug.Log ("height=" + height + ", width:" + width);
		Vector3 tempShift = new Vector3 (0.0f, 0.02f, 0.0f);

		//CREATE DUMMY CIRCLE
		createPrefab(suggestion, 0, planePosition,new Vector3(000.1f,000.1f,000.1f), "Suggestion");
		createPrefab(suggestion, 1, planePosition,new Vector3(000.1f,000.1f,000.1f), "Suggestion");
	

		//start drwing white circles
		StartCoroutine(startVisualization());

		//start drawing black circles
		createCircles (forgroundCircles, 0.5,tempShift, "forgroundCircles",suggestion ["Suggestion" + 0 +code].access ());
		changeColor ("forgroundCircles", Color.white);
		suggestion ["Suggestion" + 0 +code].access ().transform.Rotate (planeRotation.eulerAngles);
	}


	private IEnumerator startVisualization()
	{
		while (true ) {

			destroyPrefab (backgroundCircles, "backgroundCircles");
			createCircles (backgroundCircles,1.0, interaction (),"backgroundCircles",suggestion ["Suggestion" + 1 +code].access ());
			suggestion ["Suggestion" + 1 +code].access ().transform.Rotate (planeRotation.eulerAngles);
			changeColor ("backgroundCircles", Color.black);


			counter++;
			yield return new WaitForSeconds (0.0f);
		}
	}



	private void createCircles(Dictionary<string, CustomPrefab1> prefabs, double scaleFactor, Vector3 shift,string tag,GameObject parent)
	{
		du = height / nu;
		dv = width / nv;

		Vector3 tempScale = new Vector3 ((float)(du*scaleFactor), 0.1f, (float)(dv*scaleFactor));
		Random.seed = 42;
		for (int i = 0; i < nu * nv; i++) 
		{
			double iu=i/nu;
			double iv=i%nv;

			double pu= du/2.0 + planePosition.x - height/2.0 + iu*du+shift.x*Random.Range(-5.0f,5.0f)/nu;
			double pv= dv/2.0 + planePosition.z - width/2.0 + iv*dv+shift.z*Random.Range(-5.0f,5.0f)/nv;
			Vector3 tempPosition =new Vector3((float)pu , planePosition.y+shift.y , (float)pv);
			createPrefab(prefabs, i, tempPosition, tempScale, tag);
		}
		//make the game object its parent
		for(int i=0;i <prefabs.Count;i++)
		{
			prefabs [tag + i+code].access().transform.parent=parent.transform;
		}
		/*GameObject[] go = GameObject.FindGameObjectsWithTag (tag);
		{
			foreach (GameObject d in go) {
				d.transform.parent = parent.transform;
			}
		}*/
	}

	private void createPrefab(Dictionary<string, CustomPrefab1> prefabs, int c, Vector3 p, Vector3 s, string t)
	{
		string prefabName=t+c+code;
		prefabs.Add(prefabName, new CustomPrefab1(prefabName, p,s,t));
		prefabs[prefabName].Instantiate();

	}

	private void destroyPrefab(Dictionary<string, CustomPrefab1> prefabs,string tag)
	{
		for(int i=0;i <prefabs.Count;i++)
		{
			prefabs [tag + i+code].delete ();
		}
		prefabs.Clear ();
		/*
		GameObject[] go = GameObject.FindGameObjectsWithTag(tag);
		{
			foreach (GameObject d in go) 
			{
				//d.transform.localScale = new Vector3 (10.0f, 10.0f, 10.0f);
				if (d) {
					Destroy (d);
					prefabs.Clear ();
				}
			}
		}*/
	}
	private Vector3 interaction()
	{

		float shiftX =  _UtilMove.gameObject.transform.localPosition.x;
		float shiftY =   _UtilMove.gameObject.transform.localPosition.z;

		return new Vector3(shiftX,0.00f,shiftY);

	}

	private void changeColor(string tag, Color c)
	{
		GameObject[] go = GameObject.FindGameObjectsWithTag (tag);
		//Material whiteDiffuseMat = new Material(Shader.Find("WhitePlane"));

		foreach (GameObject d in go) {
			Renderer rend = d.GetComponent<Renderer> ();
			//rend.material.shader = Shader.Find ("Specular");

			rend.material.EnableKeyword("_EMISSION");
			rend.material.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;
			rend.material.SetColor ("_EmissionColor", c);
			//DynamicGI.SetEmissive (rend, Color.white);
			//rend.material.SetColor ("_SpecularColor", Color.white);
			rend.material.SetColor ("_Color", c);

		}

	}

}
