using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System;


public class PodCreator : MonoBehaviour {

	//pod prefab initialization
	[SerializeField]
	private GameObject podRed;
	[SerializeField]
	private GameObject podBlue;
	[SerializeField]
	private GameObject podGreen;
	[SerializeField]
	private GameObject podWhite;
	[SerializeField]
	private GameObject infoSphere;

	private GameObject temp;

	//pod dictionnaries initialization
	private Dictionary<string, CustomPrefab1> podsRed = new Dictionary<string, CustomPrefab1>();
	private Dictionary<string, CustomPrefab1> podsBlue = new Dictionary<string, CustomPrefab1>();
	private Dictionary<string, CustomPrefab1> podsGreen = new Dictionary<string, CustomPrefab1>();
	private Dictionary<string, CustomPrefab1> podsWhite = new Dictionary<string, CustomPrefab1>();
	private Dictionary<string, CustomPrefab1> InfoSphere = new Dictionary<string, CustomPrefab1>();

	//[SerializeField]
	private const int podsNumber = 500;

	private Behaviour halo;

	//attractor script initialization
	private Vector3 tempVectPos;
	private Vector3 tempVectScale;
	private float tempPosY;
	private float tempNum;

	//floating script
	private float movementDistance = 1.0f; // The maximum distance can move up and down

	private float range;

	//random growth initialization
	private int currentCount = 0;
	private float[] x = new float[podsNumber];
	private float[] y = new float[podsNumber];
	private float[] z = new float[podsNumber];
	private float[] r = new float[podsNumber]; // radius
	private float height;
	private float width;
	private float top;
	private Vector3 tempVec;
	private Vector3 newVec;
	private Vector3 tempDist;
	private Vector3 tempScale;
	//////////

	/// <summary>
	/// Start this instance
	/// </summary>
	// Use this for initialization
	void Start () {
		startRandomGrowth ();
	}

	void Update() {
		updateRandomGrowth ();
	}

	private void createPrefab(Dictionary<string, CustomPrefab1> prefabs, int c, Vector3 p, Vector3 s, string t, GameObject origin)
	{
		string prefabName=t+c;
		prefabs.Add(prefabName, new CustomPrefab1(prefabName, p,s,t,origin));
		prefabs[prefabName].Instantiate();
		prefabs [prefabName].access ().transform.parent = this.gameObject.transform;

	}
	private void destroyPrefab(Dictionary<string, CustomPrefab1> prefabs,string tag)
	{
		for(int i=0;i <prefabs.Count;i++)
		{
			prefabs [tag + i].delete ();
		}
		prefabs.Clear ();
	}
		
	private float attactor (float p)
	{
		float attractor=-50.0f;
		float spread=0.00f;
		float strength=35.0f;
		float dp=attractor - p;
		float dist = Mathf.Abs (dp);
		p += (dp/dist) * strength * Mathf.Exp(-spread * dist * dist);
		if (p < -50.0f) {
			p = -49.0f;
		}
		return p;

	}

	private void startRandomGrowth()
	{
		height = this.transform.localScale.x*10.0f;
		top = this.transform.localScale.y*10.0f;
		width = this.transform.localScale.z*10.0f;
		x[0] = this.transform.localPosition.x;
		y[0] = this.transform.localPosition.y;
		z[0] = this.transform.localPosition.z;
		r[0] = 10.0f;

		tempScale=new Vector3 (r[0], r[0], r[0]);
		tempVec = new Vector3 (x[0], y[0], z[0]);
		//Debug.Log (tempVec);
		createPrefab(InfoSphere, currentCount, tempVec, tempScale, "InfoSphere", infoSphere);
		currentCount++;
	}

	private void updateRandomGrowth()
	{
		if (currentCount < podsNumber) {
			// create a radom set of parameters
			//float newR = UnityEngine.Random.Range(1, 7);
			float newR = 3.0f;
			float newX = UnityEngine.Random.Range (0 + newR - height / 2.0f, newR + height / 2.0f);
			float newY = UnityEngine.Random.Range (0 + newR, newR + top);
			float newZ = UnityEngine.Random.Range (0 + newR - width / 2.0f, newR + width / 2.0f);
			newVec = new Vector3 (newX, newY, newZ);

			float closestDist = 100000000.0f;
			int closestIndex = 0;
			// which circle is the closest?
			for (int i = 0; i < currentCount; i++) {
				tempVec = new Vector3 (x [i], y [i], z [i]);
				tempDist = tempVec - newVec;
				float newDist = tempDist.magnitude;
				if (newDist < closestDist) {
					closestDist = newDist;
					closestIndex = i; 
				} 
			}
			tempVec = new Vector3 (x [closestIndex], y [closestIndex], z [closestIndex]);
			tempDist = newVec - tempVec;
			tempDist = newVec - (tempDist.normalized * (tempDist.magnitude - (r [closestIndex] + newR) / 1.5f));

			x [currentCount] = tempDist.x;
			//y [currentCount] = this.transform.localPosition.y+tempDist.y;
			y [currentCount] = tempDist.y;
			z [currentCount] = tempDist.z;
			r [currentCount] = newR;

			// draw them
			tempScale = new Vector3 (r [currentCount], r [currentCount], r [currentCount]);
			tempVec = new Vector3 (x [currentCount], y [currentCount], z [currentCount]);

			if (currentCount % 4 == 1) {
				createPrefab (podsRed, currentCount, tempVec, tempScale, "PodRed", podRed);
			} else if (currentCount % 4 == 2) {
				createPrefab (podsGreen, currentCount, tempVec, tempScale, "PodGreen", podGreen);
			} else if (currentCount % 4 == 3) {
				createPrefab (podsBlue, currentCount, tempVec, tempScale, "PodBlue", podBlue);
			} else {
				createPrefab (podsWhite, currentCount, tempVec, tempScale, "PodWhite", podWhite);
			}
			//createPrefab(growthCircles, currentCount, tempVec, tempScale, "Suggestion");
			currentCount++;
		} else {
			//MuseumManager.Instance.growing=false;
		}

	}


	//go =GameObject.Instantiate(pod)as GameObject;
	/*for (int i = 0; i < podsNumber; i++) {

			tempVectScale = new Vector3 (1.0f, 1.0f, 1.0f);
			range = 50.0f - movementDistance;

			tempNum = Random.Range (-range, range);
			tempPosY = attactor (tempNum);
			tempVectPos = new Vector3 (Random.Range (-50.0f, 50.0f), tempPosY, Random.Range (-50.0f, 50.0f));
			createPrefab(podsRed, i, this.transform.position+tempVectPos ,tempVectScale, "PodRed", podRed);

			tempNum = Random.Range (-range, range);
			tempPosY = attactor (tempNum);
			tempVectPos = new Vector3 (Random.Range (-50.0f, 50.0f), tempPosY, Random.Range (-50.0f, 50.0f));
			createPrefab(podsGreen, i, this.transform.position+tempVectPos ,tempVectScale, "PodGreen", podGreen);

			tempNum = Random.Range (-range, range);
			tempPosY = attactor (tempNum);
			tempVectPos = new Vector3 (Random.Range (-50.0f, 50.0f), tempPosY, Random.Range (-50.0f, 50.0f));
			createPrefab(podsBlue, i, this.transform.position+tempVectPos ,tempVectScale, "PodBlue", podBlue);
		}
		string tempString = "Pod" + 0;
		//temp = pods [tempString].access ().transform.GetChild(0).gameObject;
		//halo =(Behaviour)temp.gameObject.GetComponent ("Halo");
		//halo.enabled = false;
		//halo.GetType ().GetProperty ("Size").SetValue(halo, 1.0f,null);*/
}
