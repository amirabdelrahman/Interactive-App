using UnityEngine;
using System.Collections;

public class UtilMove : MonoBehaviour {

	public Vector3 destination =new Vector3(5,0,0);

	private Vector3 Speed = new Vector3 ();
	public float Acceleration = 2f;
	public float maxSpeed = 15f;
    public float destinationRadius = 5f;

	private Vector3 currentVelocity = new Vector3();

    private Vector3 vectorSpeedGoal = new Vector3();

    // Use this for initialization
    void Start () {
		
	}

	// Update is called once per frame
	void Update () {

        Vector3 center = new Vector3(20, 5, 20);

		destination = NeuralManager.Instance.point + new Vector3(0, this.transform.position.y, 0);
        //destination = center + new Vector3(0, 2.5f, 0);


        Vector3 vectorAB = destination - this.transform.position;
        float distance = vectorAB.magnitude;

        if (distance > destinationRadius)
        {
            Vector3 vectorABn = vectorAB.normalized;
            vectorSpeedGoal += vectorABn * Acceleration;
            if (vectorSpeedGoal.magnitude > maxSpeed) vectorSpeedGoal = vectorSpeedGoal.normalized * maxSpeed;
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

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(NeuralManager.Instance.point, destinationRadius);
    }
}
