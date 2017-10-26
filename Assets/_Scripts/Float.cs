using UnityEngine;
using System.Collections;

public class Float : MonoBehaviour {

	//floating declarations
	private float startingY;
	private bool isMovingUp = true;
	private float floatSpeed = 0.5f; // In cycles (up and down) per second
	private float movementDistance = 1.0f; // The maximum distance can move up and down
	private Vector3 yCorrection;

	// Use this for initialization
	void Start () {
		startingY = this.transform.position.y;

		//floating initialization and randomization
		startingY = this.transform.position.y;
		float num = UnityEngine.Random.Range (-movementDistance, movementDistance);
		yCorrection = new Vector3 (0.0f,num, 0.0f);
		this.transform.position+=yCorrection;
	}
	
	// Update is called once per frame
	void Update () {
		Float1 ();
	
	}
	private void Float1()
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
}
