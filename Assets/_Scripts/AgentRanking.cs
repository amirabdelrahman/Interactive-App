using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AgentRanking : MonoBehaviour {



	private int currentIndex=-1;
	private int currentLayer=-1;


	// Use this for initialization
	void Start () {
		if (this.gameObject.name != "Cube") {
			currentIndex = Convert.ToInt32 (this.gameObject.name.Remove (0, 7));
			currentLayer = Convert.ToInt32(this.transform.position.z);
		}

		//initial locked objects
		if (currentIndex == 0) {
			SwarmManager.Instance.target[currentLayer][currentIndex][0] = -2;
		}
		//Debug.Log (currentIndex);
	
	}
	
	// Update is called once per frame
	void Update () {
		if (currentIndex != 0) {
			getRanking ();
		}

		UpdateEdges ();

		if (SwarmManager.Instance.nextAgent[currentLayer]==currentIndex && currentIndex != 0)
		{
			getTarget ();
		}

		if (currentIndex != 0) {
			getRanking ();
		}

	
	}

	List <GameObject> currentCollisions = new List <GameObject> ();

	//void OnCollisionEnter (Collision col){
	void OnTriggerEnter (Collider col) {

		// Add the GameObject collided with to the list.
		currentCollisions.Add (col.gameObject);

	}
	//void OnCollisionExit (Collision col) {
	void OnTriggerExit (Collider col) {

		// Remove the GameObject collided with from the list.
		currentCollisions.Remove (col.gameObject);

	}

	void getRanking ()
	{

		int minimum = SwarmManager.Instance.agentsNumber+1;
		int minimumIndex = -1;

		foreach (GameObject gObject in currentCollisions) {
			if (gObject != null) {
				int index = Convert.ToInt32 (gObject.name.Remove (0, 7));

				if (SwarmManager.Instance.states [currentLayer] [index] > 0) {
					if (SwarmManager.Instance.states [currentLayer] [index] < minimum) {
						minimum = SwarmManager.Instance.states [currentLayer] [index];
						minimumIndex = index;
					}
				}
			}
			if (minimum != SwarmManager.Instance.agentsNumber + 1) {
				SwarmManager.Instance.states [currentLayer] [currentIndex] = minimum + 1;
			}
		}
	}
	void UpdateEdges()
	{
		List<int> edgeList = new List<int>();

		edgeList.Add( -1);
		edgeList.Add( -1);
		edgeList.Add( -1);
		edgeList.Add( -1);

		//Debug.Log (currentCollisions.Count);

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
			SwarmManager.Instance.edges [currentLayer] [currentIndex] = new List<int> (edgeList);
		}

		
	}

	void getTarget()
	{
		List<int> edgeList = new List<int>(SwarmManager.Instance.edges[currentLayer] [SwarmManager.Instance.target[currentLayer][currentIndex][0]]);

		for (int i=0;i<4;i++) 
		{
			if (edgeList[i] == currentIndex) 
			{
				edgeList[i] = -1;
			}

		}
		//Debug.Log ("Target"+SwarmManager.Instance.target[currentIndex][0] +" N:" + edgeList [0] + " E:" + edgeList [1] + " S:" + edgeList [2] + " W:" + edgeList [3]);

		//1
		//if(south empty and west full and north full and east full || south empty and west full and north empty and east full
		//||south empty and west full and north empty and east empty||south empty and west full and north full and east empty)
		if ((edgeList [2] == -1 && edgeList [3] != -1 && edgeList [0] != -1 && edgeList [1] != -1) || (edgeList [2] == -1 && edgeList [3] != -1 && edgeList [0] == -1 && edgeList [1] != -1)
			|| (edgeList [2] == -1 && edgeList [3] != -1 && edgeList [0] == -1 && edgeList [1] == -1) || (edgeList [2] == -1 && edgeList [3] != -1 && edgeList [0] != -1 && edgeList [1] == -1)) { 	

			//Debug.Log ("ana " + currentIndex + " hena 1");
			if (SwarmManager.Instance.edges[currentLayer] [edgeList [3]] [2] == -1) {//	If (south of the west agent empty)
				//Go south of the west agent
				SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = edgeList [3];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 2;
			} else {
				//go south
				SwarmManager.Instance.target [currentLayer][currentIndex] [0] = SwarmManager.Instance.target[currentLayer][currentIndex][0];
				//SwarmManager.Instance.target [currentIndex] [0] = currentIndex;
				SwarmManager.Instance.target [currentLayer][currentIndex] [1] = -1;

				if (SwarmManager.Instance.edges[currentLayer] [SwarmManager.Instance.target [currentLayer][currentIndex] [0]] [2] == currentIndex) {
					SwarmManager.Instance.target [currentLayer][currentIndex] [0] = currentIndex;
				} else {
					SwarmManager.Instance.target [currentLayer][currentIndex] [1] = 2;
				}
			}

		}
		//2
		//Else if(south empty and west empty and north full and east full || south full and west empty and north empty and east empty
		//||south full and west empty and north full and east full||south full and west empty and north full and east empty)
		//|| south empty and west empty and north full and east empty
		else if ((edgeList [2] == -1 && edgeList [3] == -1 && edgeList [0] != -1 && edgeList [1] != -1) 
			||(edgeList [2] == -1 && edgeList [3] == -1 && edgeList [0] != -1 && edgeList [1] == -1)
			|| (edgeList [2] != -1 && edgeList [3] == -1 && edgeList [0] != -1 && edgeList [1] != -1) 
			|| (edgeList [2] != -1 && edgeList [3] == -1 && edgeList [0] != -1 && edgeList [1] == -1)) {

			//Debug.Log ("ana " + currentIndex + " hena 2");

			if (SwarmManager.Instance.edges[currentLayer] [edgeList [0]] [3] == -1) {//If (west of the north agent empty)
				//Go west of the north agent
				SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = edgeList [0];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 3;

			} else {
				//Go west 
				SwarmManager.Instance.target [currentLayer][currentIndex] [0] = SwarmManager.Instance.target[currentLayer][currentIndex][0];
				SwarmManager.Instance.target [currentLayer][currentIndex] [1] = -1;

				if (SwarmManager.Instance.edges[currentLayer] [SwarmManager.Instance.target[currentLayer] [currentIndex] [0]] [3] == currentIndex) {
					SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = currentIndex;
				} else {
					SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 3;
				}
			}
		}
		//3
		//else if(south empty and west empty and north empty and east full||south full and west full and north empty and east full
		//||south full and west empty and north empty and east full
		else if ((edgeList [2] == -1 && edgeList [3] == -1 && edgeList [0] == -1 && edgeList [1] != -1) || (edgeList [2] != -1 && edgeList [3] != -1 && edgeList [0] == -1 && edgeList [1] != -1)
			|| (edgeList [2] != -1 && edgeList [3] == -1 && edgeList [0] == -1 && edgeList [1] != -1)) {

			//Debug.Log ("ana " + currentIndex + " hena 3");

			if (SwarmManager.Instance.edges[currentLayer] [edgeList [1]] [0] == -1) {//If (north of the east agent empty)
				//Go north of the east agent
				SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = edgeList [1];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 0;

			} else {
				//Go north 
				//SwarmManager.Instance.target [currentIndex] [0] = SwarmManager.Instance.target[currentIndex][0];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = -1;

				if (SwarmManager.Instance.edges[currentLayer] [SwarmManager.Instance.target[currentLayer] [currentIndex] [0]] [0] == currentIndex) {

					SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = currentIndex;
					//Debug.Log ("YARAAB!!!");
				} else 
				{
					SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 0;
				}
			}
		}
		//4
		//(south full and west full and north empty and east empty) Else if (south full and west full and north full and east empty) 
		else if ((edgeList [2] != -1 && edgeList [3] != -1 && edgeList [0] == -1 && edgeList [1] == -1) || (edgeList [2] != -1 && edgeList [3] != -1 && edgeList [0] != -1 && edgeList [1] == -1)
			||(edgeList [2] != -1 && edgeList [3] == -1 && edgeList [0] == -1 && edgeList [1] == -1)) 
		{
			//Debug.Log ("ana " + currentIndex + " hena 4");

			if (SwarmManager.Instance.edges [currentLayer][edgeList [2]] [1] == -1) {//	If (east of the south agent empty)
				//Go east of the south agent
				SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = edgeList [2];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 1;
			} 
			else 
			{
				//Go east 
				SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = SwarmManager.Instance.target[currentLayer][currentIndex][0];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = -1;

				if (SwarmManager.Instance.edges[currentLayer] [SwarmManager.Instance.target[currentLayer] [currentIndex] [0]] [1] == currentIndex) {
					SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = currentIndex;
				} else {
					SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 1;
				}
			}
		}
		else 
		{
			Debug.Log ("henaaaa!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
			SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = SwarmManager.Instance.target[currentLayer][currentIndex][0];
			SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 0;

		}



		/*List<int> edgeList = new List<int>(SwarmManager.Instance.edges[currentLayer] [SwarmManager.Instance.target[currentLayer][currentIndex][0]]);

		for (int i=0;i<4;i++) 
		{
			if (edgeList[i] == currentIndex) 
			{
				edgeList[i] = -1;
			}
			
		}
		if (currentLayer == 0) {
			Debug.Log ("Target" + SwarmManager.Instance.target [currentLayer] [currentIndex] [0] + " N:" + edgeList [0] + " E:" + edgeList [1] + " S:" + edgeList [2] + " W:" + edgeList [3]);
		}

		//if (currentLayer == 0) 
		{
			//Debug.Log (currentLayer);
			//Debug.Log ("Target" + SwarmManager.Instance.target [currentLayer] [currentIndex] [0] + " N:" + edgeList [0] + " E:" + edgeList [1] + " S:" + edgeList [2] + " W:" + edgeList [3]);
		}

		//1
		//if(south empty and west full and north full and east full || south empty and west full and north empty and east full
		//||south empty and west full and north empty and east empty||south empty and west full and north full and east empty)
		if ((edgeList [2] == -1 && edgeList [3] != -1 && edgeList [0] != -1 && edgeList [1] != -1) || (edgeList [2] == -1 && edgeList [3] != -1 && edgeList [0] == -1 && edgeList [1] != -1)
		   || (edgeList [2] == -1 && edgeList [3] != -1 && edgeList [0] == -1 && edgeList [1] == -1) || (edgeList [2] == -1 && edgeList [3] != -1 && edgeList [0] != -1 && edgeList [1] == -1)) { 	

			//Debug.Log ("ana " + currentIndex + " hena 1");
			if (SwarmManager.Instance.edges[currentLayer] [edgeList [3]] [2] == -1) {//	If (south of the west agent empty)
				//Go south of the west agent
				SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = edgeList [3];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 2;

			} else {
				//go south
				//SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = SwarmManager.Instance.target[currentLayer][currentIndex][0];
				//SwarmManager.Instance.target [currentIndex] [0] = currentIndex;
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = -1;

				if (SwarmManager.Instance.edges[currentLayer] [SwarmManager.Instance.target[currentLayer] [currentIndex] [0]] [2] == currentIndex) {
					SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = currentIndex;
				} else {
					SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 2;
				}
			}

		}
		//2
		//Else if(south empty and west empty and north full and east full || south full and west empty and north empty and east empty
		//||south full and west empty and north full and east full||south full and west empty and north full and east empty)
		//|| south empty and west empty and north full and east empty
		else if ((edgeList [2] == -1 && edgeList [3] == -1 && edgeList [0] != -1 && edgeList [1] != -1) 
			||(edgeList [2] == -1 && edgeList [3] == -1 && edgeList [0] != -1 && edgeList [1] == -1)
		        || (edgeList [2] != -1 && edgeList [3] == -1 && edgeList [0] != -1 && edgeList [1] != -1) 
			|| (edgeList [2] != -1 && edgeList [3] == -1 && edgeList [0] != -1 && edgeList [1] == -1)) {

			//Debug.Log ("ana " + currentIndex + " hena 2");

			if (SwarmManager.Instance.edges[currentLayer] [edgeList [0]] [3] == -1) {//If (west of the north agent empty)
				//Go west of the north agent
				SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = edgeList [0];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 3;

			} else {
				//Go west 
				//SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = SwarmManager.Instance.target[currentLayer][currentIndex][0];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = -1;

				if (SwarmManager.Instance.edges[currentLayer] [SwarmManager.Instance.target[currentLayer] [currentIndex] [0]] [3] == currentIndex) {
					SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = currentIndex;
				} else {
					SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 3;
				}
			}
		}
		//3
		//else if(south empty and west empty and north empty and east full||south full and west full and north empty and east full
		//||south full and west empty and north empty and east full
		else if ((edgeList [2] == -1 && edgeList [3] == -1 && edgeList [0] == -1 && edgeList [1] != -1) || (edgeList [2] != -1 && edgeList [3] != -1 && edgeList [0] == -1 && edgeList [1] != -1)
		        || (edgeList [2] != -1 && edgeList [3] == -1 && edgeList [0] == -1 && edgeList [1] != -1)) {

			//Debug.Log ("ana " + currentIndex + " hena 3");

			if (SwarmManager.Instance.edges[currentLayer] [edgeList [1]] [0] == -1) {//If (north of the east agent empty)
				//Go north of the east agent
				SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = edgeList [1];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 0;

			} else {
				//Go north 
				SwarmManager.Instance.target [currentLayer][currentIndex] [1] = -1;

				if (SwarmManager.Instance.edges[currentLayer] [SwarmManager.Instance.target[currentLayer] [currentIndex] [0]] [0] == currentIndex) {
					
					SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = currentIndex;
					//Debug.Log ("YARAAB!!!");
				} else 
				{
					SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 0;
				}
			}
		}
		//4
		//(south full and west full and north empty and east empty) Else if (south full and west full and north full and east empty) 
		else if ((edgeList [2] != -1 && edgeList [3] != -1 && edgeList [0] == -1 && edgeList [1] == -1) || (edgeList [2] != -1 && edgeList [3] != -1 && edgeList [0] != -1 && edgeList [1] == -1)
			||(edgeList [2] != -1 && edgeList [3] == -1 && edgeList [0] == -1 && edgeList [1] == -1)) 
		{
			//Debug.Log ("ana " + currentIndex + " hena 4");

			if (SwarmManager.Instance.edges[currentLayer] [edgeList [2]] [1] == -1) {//	If (east of the south agent empty)
				//Go east of the south agent
				SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = edgeList [2];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 1;
			} 
			else 
			{
				//Go east 
				//SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = SwarmManager.Instance.target[currentLayer][currentIndex][0];
				SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = -1;

				if (SwarmManager.Instance.edges[currentLayer] [SwarmManager.Instance.target[currentLayer] [currentIndex] [0]] [1] == currentIndex) {
					SwarmManager.Instance.target[currentLayer] [currentIndex] [0] = currentIndex;
				} else {
					SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 1;
				}
			}
		}
		else 
		{
			Debug.Log ("henaaaa!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
			SwarmManager.Instance.target[currentLayer] [currentIndex] [1] = 0;

		}
			

		//Debug.Log ("Target: "+SwarmManager.Instance.target[currentIndex][0]+" N:" + edgeList [0] + " E:" + edgeList [1] + " S:" + edgeList [2] + " W:" + edgeList [3]);

		//Debug.Log("1layer " + 0 +"Target:"+SwarmManager.Instance.target[0][currentIndex][0]);
		//Debug.Log("1 and layer " + 1 +"Target:"+SwarmManager.Instance.target[1][currentIndex][0]);*/
	}
}
