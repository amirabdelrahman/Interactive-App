using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PodMovement : MonoBehaviour {

	//clicked variables
	[SerializeField]
	private PodRaycast _PodRaycast;

	private GameObject mainCamera;
	private float distance=2.0f;
	private float smooth=10.0f;



	//spiral variables
	//[SerializeField]
	private double maxTime=8.0;
	private double timeRemaining;
	private double u=1.2;
	private double v=6.0;
	private double k=0.15;

	//movement variables
	private GameObject player;
	private GameObject infoSphere;
	private double ditanceToStart=10.0;

	//smoothmovement variables
	private float destinationRadius = 0.1f;
	private Vector3 Speed = new Vector3 ();

	//general variables to check distance
	private Vector3 initialTarget =new Vector3();
	private Vector3 endTarget =new Vector3();
	private Vector3 originalPosition =new Vector3();

	private Vector3 tempVector;
	private float tempDistance  =new float();

	private Vector3 yCorrection;

	Renderer rend;

	//status booleans
	private bool atStart = false;
	private bool atEnd = false;
	private bool start = false;
	private bool active = false;
	private bool infoActive = false;
	//info variables
	private Vector3 infoPos = new Vector3(0.0f,0.0f,32.0f);
	private double infoRad=10.0;

	//floating declarations
	private float startingY;
	private bool isMovingUp = true;
	private float floatSpeed = 0.5f; // In cycles (up and down) per second
	private float movementDistance = 1.0f; // The maximum distance can move up and down

	private Behaviour meshRenderer;

	// Use this for initialization
	void Start () {
		
		//floating initialization and randomization
		startingY = this.transform.position.y;
		float num = UnityEngine.Random.Range (-movementDistance, movementDistance);
		yCorrection = new Vector3 (0.0f,num, 0.0f);
		this.transform.position+=yCorrection;
		yCorrection = new Vector3 (0.0f, 0.0f, 0.0f);

		//movement initialization
		player = GameObject.FindGameObjectWithTag ("Player");
		originalPosition=this.gameObject.transform.position;

		//spiral initialization
		timeRemaining = maxTime;

		//info initialization
		infoSphere=GameObject.FindGameObjectWithTag ("InfoSphere");
		infoPos = infoSphere.transform.position;
		rend = infoSphere.GetComponent<Renderer>();

	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	// Update is called once per frame
	void Update () {

		//check if in info space
		Vector3 tempVector1 = player.transform.position;
		tempVector = tempVector1 - infoPos;
		tempDistance = tempVector.magnitude;
		if (tempDistance < infoRad) 
		{
			//MuseumManager.Instance.info = true;

			//when close close info light
			if(infoActive)
			{
				
				//infoSphere=GameObject.FindGameObjectWithTag ("InfoSphere");
				infoActive = false;
				//infoSphere.SetActive (infoActive);
				rend.enabled = infoActive;
			}

			smoothMovement (infoScript ());
		} 
		else 
		{
			//MuseumManager.Instance.info = false;
			//when far open info light
			if(! infoActive)
			{
				
				infoActive = true;
				//infoSphere.SetActive (infoActive);
				rend.enabled = infoActive;
			}

			//opening position (not in info space)
			if (!start) 
			{
				//float in space

				smoothMovement (originalPosition);
				Float ();

				tempVector = this.transform.position + yCorrection - (player.transform.position + yCorrection);
				tempDistance = tempVector.magnitude;
				//if (tempDistance < ditanceToStart)
				if(_PodRaycast.clicked)
				{
					//MuseumManager.Instance.coming = true;
					//MuseumManager.Instance.podTag = this.gameObject.tag;
					start = true;
					//Debug.Log ("Start");
				}
			}

			if (start) 
			{
				//test if at start position
				//initialTarget = getSpiralPosition (maxTime) + (player.transform.position + yCorrection);
				initialTarget = (player.transform.position + yCorrection);
				tempVector = this.transform.position - initialTarget;
				tempDistance = tempVector.magnitude;
				if (tempDistance < 0.2f) 
				{
					atStart = true;
				}
				//test if at end position
				endTarget = getSpiralPosition (0.0) + (player.transform.position + yCorrection);
				tempVector = this.transform.position - endTarget;
				tempDistance = tempVector.magnitude;
				if (tempDistance < 0.2f) 
				{
					//Debug.Log ("at end");
					atEnd = true;
				}

				if (!atStart && !atEnd) 
				{
					smoothMovement (initialTarget);
				} 
				else if (atStart && !atEnd) 
				{
					if (timeRemaining > 0) 
					{
						createSpiralMovement ();
					}
				} 
				else if (atEnd) 
				{
					//MuseumManager.Instance.coming = false;
					//MuseumManager.Instance.click = true;

					mainCamera = GameObject.FindWithTag("MainCamera");

					this.transform.position = Vector3.Lerp (this.transform.position, mainCamera.transform.position + mainCamera.transform.forward * distance,Time.deltaTime * smooth);
					_PodRaycast.playVideo = true;
					//smoothMovement (originalPosition);
					tempVector = this.transform.position - originalPosition;
					tempDistance = tempVector.magnitude;
					if (tempDistance < 0.1f) 
					{
						//Debug.Log ("reset");
						start = false;
						atStart = false;
						atEnd = false;
						timeRemaining = maxTime;
						_PodRaycast.playVideo = false;
					}
				}
			}
		}
	}

	//private IEnumerator createSpiralMovement()
	private void createSpiralMovement()
	{
		Vector3 target= getSpiralPosition(timeRemaining);
		this.gameObject.transform.position = target+(player.transform.position + yCorrection);
		timeRemaining -= (Time.deltaTime*1.0) ;
	}

	private Vector3 getSpiralPosition(double time)
	{
		double t = rad(map (time, 0.0, maxTime, 0.0, 360.0));
		double x = u*Math.Exp(k*t)*Math.Cos((v-0.0)*t);
		double y = map (t, 0.0,2.0*Math.PI , 0.0, player.transform.localScale.y);
		double z = u*Math.Exp(k*t)*Math.Sin(v*t);
		return new Vector3 ((float)x,(float)y,(float)z);
	}

	private double map(double s, double a1, double a2, double b1, double b2)
	{
		return b1 + (s-a1)*(b2-b1)/(a2-a1);
	}

	private double rad(double degree)
	{

		return degree * (Math.PI / 180.0);
	}


	private void smoothMovement(Vector3 destination)
	{
		//vector AB
		Vector3 vectorAB = destination - this.transform.position;
		float distance = vectorAB.magnitude;
		float Acceleration = 12f;
		float maxSpeed = 40f;
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

	private void Float()
	{
		float newY = this.transform.position.y + (isMovingUp ? 1 : -1) * 2 * movementDistance * floatSpeed * Time.deltaTime;

		if (newY > startingY + movementDistance)
		{
			newY = startingY + movementDistance;
			isMovingUp = false;
		}
		else if (newY < startingY)
		{
			newY = startingY;
			isMovingUp = true;
		}
		this.transform.position = new Vector3(this.transform.position.x, newY, this.transform.position.z);
	}

	private Vector3 infoScript ()
	{
		Vector3 t;
		if (this.gameObject.tag == "PodBlue") 
		{
			//tempVector = new Vector3 ((float)map (originalPosition.x, -50.0, 50.0, -50.0, -16.6), originalPosition.y, originalPosition.z);
			t = new Vector3 ((float)map (originalPosition.x, -50.0, 50.0, -50.0, 0.0), originalPosition.y, (float)map (originalPosition.z, -50.0, 50.0, -50.0, 0.0));
		} 
		else if (this.gameObject.tag == "PodRed") 
		{
			//tempVector = new Vector3 ((float)map (originalPosition.x, -50.0, 50.0, -16.6, 16.6), originalPosition.y, originalPosition.z);
			t = new Vector3 ((float)map (originalPosition.x, -50.0, 50.0, 0.0, 50.0), originalPosition.y, (float)map (originalPosition.z, -50.0, 50.0, -50.0, 0.0));

		} 
		else if (this.gameObject.tag == "PodGreen") 
		{
			//tempVector = new Vector3 ((float)map (originalPosition.x, -50.0, 50.0, 16.6, 50.0), originalPosition.y, originalPosition.z);
			t = new Vector3 ((float)map (originalPosition.x, -50.0, 50.0, -50.0, 0.0), originalPosition.y, (float)map (originalPosition.z, -50.0, 50.0, 0.0, 50.0));
		}
		else  
		{
			t = new Vector3 ((float)map (originalPosition.x, -50.0, 50.0, 0.0, 50.0), originalPosition.y, (float)map (originalPosition.z, -50.0, 50.0, 0.0, 50.0));
		}
		return t;
	}

}
