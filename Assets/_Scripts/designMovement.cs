using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class designMovement : MonoBehaviour {

	private double timeRemaining;

	//[SerializeField]
	private double maxTime=8.0;

	//[SerializeField]
	private float ditanceToStart=5.0f;


	//[SerializeField]
	private double u=1.2;
	//[SerializeField]
	private double v=6.0;
	//[SerializeField]
	private double k=0.15;


	[SerializeField]
	private GameObject player;

	//[SerializeField]
	private float destinationRadius = 0.1f;

	private Vector3 Speed = new Vector3 ();

	private Vector3 initialTarget =new Vector3();
	private Vector3 endTarget =new Vector3();
	private Vector3 originalPosition =new Vector3();

	private Vector3 tempVector =new Vector3();
	private float tempDistance  =new float();

	private bool atStart = false;
	private bool atEnd = false;
	private bool start = false;


	// Use this for initialization
	void Start () {

		//timeRemaining = maxTime;
		//StartCoroutine (createSpiralMovement ());
		originalPosition=this.gameObject.transform.position;
		timeRemaining = maxTime;
	}
	
	// Update is called once per frame
	void Update () {

		if (!start) {
			tempVector = this.transform.position - player.transform.position;
			tempDistance = tempVector.magnitude;
			if (tempDistance < ditanceToStart) {
				start = true;
				//timeRemaining = maxTime;
			}
		}

		if (start) {
			
			//test if at start position
			initialTarget = getSpiralPosition (maxTime) + player.transform.position;
			Vector3 tempVector = this.transform.position - initialTarget;
			tempDistance = tempVector.magnitude;
			if (tempDistance < 0.2f) {
				atStart = true;
			}
			//test if at end position
			endTarget = getSpiralPosition (0.0) + player.transform.position;
			tempVector = this.transform.position - endTarget;
			tempDistance = tempVector.magnitude;
			if (tempDistance < 0.2f) {
				Debug.Log ("at end");
				atEnd = true;
			}

			if (!atStart && !atEnd) {
				smoothMovement (initialTarget);
				//atStart = true;
			} else if (atStart && !atEnd) {
			//}else{
				if (timeRemaining > 0) {
					createSpiralMovement ();
					//Debug.Log ("spiral");
				}
			} else if (atEnd) {
				smoothMovement (originalPosition);
				//Debug.Log ("at end..");

				tempVector = this.transform.position - originalPosition;
				tempDistance = tempVector.magnitude;
				if (tempDistance <0.1f) {
					Debug.Log ("reset");
					start = false;
					atStart = false;
					atEnd = false;
					timeRemaining = maxTime;
				}

			}


			//if (Input.GetKey (KeyCode.Space)) {
				//timeRemaining = maxTime;
			//}
		}

	}
	//private IEnumerator createSpiralMovement()
	private void createSpiralMovement()
	{
		//while (true && timeRemaining>0.0) {
		Vector3 target= getSpiralPosition(timeRemaining);
		//Debug.Log (target);
		this.gameObject.transform.position = target+player.transform.position;
		timeRemaining -= (Time.deltaTime*0.75) ;
		//Debug.Log (timeRemaining);
			//yield return new WaitForSeconds (delay);
		//}

	}

	private Vector3 getSpiralPosition(double time)
	{
		//UnityEngine.Random.seed = 42;
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
		if (distance > destinationRadius)
		{
			Vector3 vectorABn = vectorAB.normalized;
			vectorSpeedGoal += vectorABn * Acceleration;
			if (vectorSpeedGoal.magnitude > maxSpeed) 
				vectorSpeedGoal = vectorSpeedGoal.normalized * maxSpeed;
		}
		else
		{
			vectorSpeedGoal *= 0.95f;
		}

		this.Speed = Vector3.SmoothDamp(this.Speed, vectorSpeedGoal, ref currentVelocity, 0.4f);


		float distanceToDestination = (destination - this.transform.position).magnitude;
		//Debug.Log(distanceToDestination);
		if (distanceToDestination > 0) {
			this.transform.position += this.Speed * Time.deltaTime;
		}
	}
		
}
