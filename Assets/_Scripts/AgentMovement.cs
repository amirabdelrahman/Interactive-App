using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AgentMovement : MonoBehaviour {

	private int currentIndex=-1;

	[SerializeField]
	private PodRaycast _PodRaycast;

	private GameObject mainCamera;
	private float distance=2.0f;
	private float smooth=10.0f;

	//movement variables
	private GameObject player;
	private GameObject infoSphere;
	private double ditanceToStart=10.0;

	//smoothmovement variables
	private float destinationRadius = 0.1f;
	private Vector3 Speed = new Vector3 ();
	[SerializeField]
	float Acceleration = 70f;
	[SerializeField]
	float maxSpeed = 200f;

	//general variables to check distance
	private Vector3 initialTarget =new Vector3();
	private Vector3 endTarget =new Vector3();
	private Vector3 originalPosition =new Vector3();

	private Vector3 tempVector;
	private float tempDistance  =new float();

	//status booleans
	private bool atStart = false;
	private bool atEnd = false;
	private bool start = false;
	private bool active = false;
	private bool next = false;

	private bool inGeometry=false;
	private bool justOut=false;

	private Rigidbody rb = new Rigidbody();
	private Renderer r = new Renderer ();

	//floating declarations
	private float startingY;
	private bool isMovingUp = true;
	private float floatSpeed = 0.5f; // In cycles (up and down) per second
	private float movementDistance = 1.0f; // The maximum distance can move up and down

	private int currentLayer;

	// Use this for initialization
	void Start () {
		
		if (this.gameObject.name != "Cube") {
			currentIndex = Convert.ToInt32 (this.gameObject.name.Remove (0, 7));
			currentLayer = Convert.ToInt32(this.transform.position.z);
		}


		r = this.gameObject.GetComponent<Renderer>();


	}
	
	// Update is called once per frame
	void Update () {

		//test to see if the 
		if (!start && SwarmManager.Instance.nextAgent [currentLayer] == currentIndex && currentIndex != 0 && !SwarmManager.Instance.locked [currentLayer] [currentIndex]) {
			SwarmManager.Instance.agentMoving [currentLayer] = true;
			start = true;
			r.material.color = new Color (255f / 255f, 163f / 255f, 43f / 255f);
		} else {//check if already in geometry lock
			Vector3 center = new Vector3 (0.0f,0.0f,(float) currentLayer);
			float distance = Vector3.Distance (this.transform.position,center);
			if (distance < (float)currentLayer && this.transform.position.y > 0.0f) {
				SwarmManager.Instance.locked [currentLayer][currentIndex]= true;
				SwarmManager.Instance.fillAgent.Add (this.transform.position);
				r.material.color = new Color (142f / 255f, 229f/ 255f, 220f/ 255f);
			}
		}


		if (SwarmManager.Instance.target[currentLayer][currentIndex][0]==currentIndex) {
			UpdateEdges ();
		}

		//move
		if (start) 
		{
			//Debug.Log("Agent "+currentIndex +" moving");

			UpdateEdges (); //to be in sync

			if (currentIndex != 0) {
				getRanking ();
			}

			initialTarget = getTargetPosition ();

			testEnd();

			if (!atEnd) 
			{
				//Move Agent
				this.transform.position = initialTarget;
				//smoothMovement (initialTarget);

				//check if not finished
				if (this.transform.position.y < 0) {
					//mark layer finished
					SwarmManager.Instance.nextAgent[currentLayer]=-2;
					start = false;
				}

			}
			else //agent at end
			{
				
				//lock agent 
				SwarmManager.Instance.locked[currentLayer][currentIndex] = true;
				SwarmManager.Instance.fillAgent.Add (this.transform.position);
				//change color to blue
				r.material.color = new Color (142f / 255f, 229f/ 255f, 220f/ 255f);


				start = false;
				//Debug.Log ("Agent:"+currentIndex+" arrived!");
				if (!next) {
					SwarmManager.Instance.agentMoving[currentLayer] = false;
					next = true;
				}
			}
		}
	
	}

	//test if at end position
	private void testEnd()
	{
		//test if in geometry

		Vector3 center = new Vector3 (0.0f,0.0f,(float) currentLayer);
		float distance = Vector3.Distance (this.transform.position,center);

		//Debug.Log (distance);
		//if (this.transform.position.x < 0 && this.transform.position.x >= -20 && this.transform.position.y < 5.0f-(float)currentLayer/2.0f && this.transform.position.y > 0.0f) 
		
		if (distance<(float) currentLayer&& this.transform.position.y > 0.0f) 
		{
			//Debug.Log (this.name+" inGeometry");
			inGeometry = true;

			foreach (int edge in SwarmManager.Instance.edges[currentLayer][currentIndex]) 
			{
				if (edge != -1) 
				{
					if (SwarmManager.Instance.locked[currentLayer][edge] == true && SwarmManager.Instance.states[currentLayer] [currentIndex] == SwarmManager.Instance.states[currentLayer] [edge]) 
					{
						Debug.Log ("current state"+SwarmManager.Instance.states[currentLayer] [currentIndex]+"locked neighbor"+ edge+":"+SwarmManager.Instance.states[currentLayer]  [edge]);
						atEnd = true;
					}
				}
			}
		} 
		else 
		{
			if (inGeometry) 
			{
				justOut = true;

			}
		}

		if (justOut) 
		{
			atEnd = true;
		}


	}

	private void smoothMovement(Vector3 destination)
	{
		//vector AB
		Vector3 vectorAB = destination - this.transform.position;
		float distance = vectorAB.magnitude;

		Vector3 currentVelocity = new Vector3();


		Vector3 vectorSpeedGoal = new Vector3();
		if (distance > destinationRadius){
			Vector3 vectorABn = vectorAB.normalized;
			vectorSpeedGoal += vectorABn * Acceleration;
			if (vectorSpeedGoal.magnitude > maxSpeed) 
				vectorSpeedGoal = vectorSpeedGoal.normalized * maxSpeed;
		}
		else {
			vectorSpeedGoal *= 0.95f;
		}

		this.Speed = Vector3.SmoothDamp(this.Speed, vectorSpeedGoal, ref currentVelocity, 0.4f);

		float distanceToDestination = (destination - this.transform.position).magnitude;
		if (distanceToDestination > 0) {
			this.transform.position += this.Speed * Time.deltaTime;
		}
	}



	private Vector3 getTargetPosition()
	{
		string n = "Agents" + currentLayer + "" + SwarmManager.Instance.target [currentLayer] [currentIndex] [0];

		//Debug.Log("Agent:"+currentIndex+" on layer:"+ currentLayer+" target:"+n);


		Vector3 targetPosition = GameObject.Find(n).transform.position;
		int direction = SwarmManager.Instance.target[currentLayer][currentIndex][1];


		if (direction == 0) {//north
			targetPosition= targetPosition+ new Vector3(0.0f,-1f,0.0f);
			//Debug.Log ("Agent: "+currentIndex +" Target:"+SwarmManager.Instance.target [currentLayer][currentIndex][0]+" North");
		}
		else if (direction == 1) {//east
			targetPosition= targetPosition+ new Vector3(1f,0.0f,0.0f);
			//Debug.Log ("Agent: "+currentIndex +" Target:"+SwarmManager.Instance.target [currentLayer][currentIndex][0]+" east");
		}
		else if (direction == 2) {//south
			targetPosition= targetPosition + new Vector3(0.0f,1f,0.0f);
			//Debug.Log ("Agent: "+currentIndex +"  Target:"+SwarmManager.Instance.target [currentLayer][currentIndex][0]+" south");
		}
		else if (direction == 3) {//west
			targetPosition=targetPosition + new Vector3(-1f,0.0f,0.0f);
			//Debug.Log ("Agent: "+currentIndex +"  Target:"+SwarmManager.Instance.target [currentLayer][currentIndex][0]+" west");
		}
		else { //no empty edges
			targetPosition=this.transform.position;
		}

		return targetPosition;
	}

	List <GameObject> currentCollisions = new List <GameObject> ();

	void OnTriggerEnter (Collider col) {
		currentCollisions.Add (col.gameObject);

	}
	void OnTriggerExit (Collider col) {
		currentCollisions.Remove (col.gameObject);

	}

	void getRanking ()
	{

		int minimum = SwarmManager.Instance.agentsNumber+1;
		int minimumIndex = -1;

		foreach (GameObject gObject in currentCollisions) 
		{
			int index = Convert.ToInt32(gObject.name.Remove (0, 7));

			if (SwarmManager.Instance.states[currentLayer] [index] > 0) 
			{
				if (SwarmManager.Instance.states [currentLayer][index] < minimum) 
				{
					minimum = SwarmManager.Instance.states[currentLayer] [index];
					minimumIndex = index;
				}
			}
		}
		if (minimum != SwarmManager.Instance.agentsNumber + 1) {
			SwarmManager.Instance.states[currentLayer] [currentIndex] = minimum+1;
		}
	}

	void UpdateEdges()
	{
		List<int> edgeList = new List<int>();

		edgeList.Add( -1);
		edgeList.Add( -1);
		edgeList.Add( -1);
		edgeList.Add( -1);

		if (currentCollisions != null) {
			foreach (GameObject gObject in currentCollisions) {

				if (gObject != null) {
					int index = Convert.ToInt32 (gObject.name.Remove (0, 7));

					bool north = false;
					bool east = false;
					bool south = false;
					bool west = false;

					//if north
					if (gObject.transform.position.y - this.transform.position.y <= -0.5f) {
						north = true;
					} //else if south
					if (gObject.transform.position.y - this.transform.position.y >= 0.5f) {
						south = true;
					}//else if east
					if (gObject.transform.position.x - this.transform.position.x <= -0.5f) {
						west = true;
					}//else if west
					if (gObject.transform.position.x - this.transform.position.x >= 0.5f) {
						east = true;
					}

					if (north && !east && !west) {
						edgeList [0] = index;
					} //else if south
			
					else if (south && !east && !west) {
						edgeList [2] = index;
					}//else if east
			
					else if (east && !north && !south) {
						edgeList [1] = index;
					}//else if west
			
					else if (west && !north && !south) {
						edgeList [3] = index;
					}
				}
			}
		}
		SwarmManager.Instance.edges[currentLayer] [currentIndex] = new List<int>(edgeList);
		//if (SwarmManager.Instance.nextAgent[currentLayer] == currentIndex) {
			//Debug.Log ("Update edge");
		//}

	}


}
