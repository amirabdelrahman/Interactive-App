using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class GenerateMesh : MonoBehaviour {

	int size = 40;

	float[,,] data;

	//When an edge transitions between a positive and negative value, it'll be marked as "crossed"
	public float surfaceCrossValue = 0;

	//The sacle of the noise for input into the system
	public float noiseScaleFactor = 20;

	private GameObject player;

	Mesh localMesh;

	MeshFilter meshFilter;

	Vector3 previousPosition;

	public int brushSize;

	private int currentCount=0;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player");
		previousPosition = player.transform.position;

		localMesh = new Mesh();
		meshFilter = GetComponent<MeshFilter>();

		data = new float[size,size,size];

		FillData(transform.position.x, transform.position.y, transform.position.z);
		ApplyDataToMesh();
	}

	// Update is called once per frame
	void Update () {
		bool changed = false;
		bool changedMeshOnly = false;

		if(Input.GetKey(KeyCode.Q)) {
			surfaceCrossValue += .01f;
			changedMeshOnly = true;
		}

		if(Input.GetKey(KeyCode.E)) {
			surfaceCrossValue -= .01f;
			changedMeshOnly = true;
		}

		if(Input.GetKey(KeyCode.A)) {
			Camera.main.transform.Translate(-.5f, 0, 0, Space.World);
			this.transform.Translate(-.5f, 0, 0, Space.World);
			changed = true;
		}

		if(Input.GetKey(KeyCode.D)) {
			Camera.main.transform.Translate(.5f, 0, 0, Space.World);
			this.transform.Translate(.5f, 0, 0, Space.World);
			changed = true;
		}

		if(Input.GetKey(KeyCode.S)) {
			Camera.main.transform.Translate(0, 0, -.5f, Space.World);
			this.transform.Translate(0, 0, -.5f, Space.World);
			changed = true;
		}

		if(Input.GetKey(KeyCode.W)) {
			Camera.main.transform.Translate(0, 0, .5f, Space.World);
			this.transform.Translate(0, 0, .5f, Space.World);
			changed = true;
		}

		if(Input.GetKey(KeyCode.R)) {
			noiseScaleFactor += .1f;
			changed = true;
		}

		if(Input.GetKey(KeyCode.F)) {
			noiseScaleFactor -= .1f;
			changed = true;
		}

		//if (Vector3.Distance(previousPosition, player.transform.position) > 1)
		{
			//brushTool();
			//ApplyDataToMesh();
		}

		if (changed || changedMeshOnly){
			if(changed)
				FillData(transform.position.x, transform.position.y, transform.position.z);

			ApplyDataToMesh();
		}

		int count = SwarmManager.Instance.fillAgent.Count;
		if (count> currentCount) {
			//Debug.Log("curr:"+currentCount+" count:"+count);
			for (int i = currentCount ; i < count; i++) {
				int x=Convert.ToInt32(SwarmManager.Instance.fillAgent[i].x+10);
				int y=Convert.ToInt32(SwarmManager.Instance.fillAgent[i].y);
				int z=Convert.ToInt32(SwarmManager.Instance.fillAgent[i].z);

				//Debug.Log (x + " " + y + " " + z + " ");
				//FillData (SwarmManager.Instance.fillAgent[i].x+10,SwarmManager.Instance.fillAgent[i].y,SwarmManager.Instance.fillAgent[i].z);
				data [x,y,z] = 1.0f;
				//FillData(transform.position.x, transform.position.y, transform.position.z);
			}

			ApplyDataToMesh();
			currentCount = count;
		}

	}

	void ApplyDataToMesh() {
		TerrainMeshGenerator.FillMesh(ref localMesh, data, size, size, surfaceCrossValue);
		meshFilter.mesh = localMesh;
	}

	void brushTool()
	{

		Vector3 playerPosition = player.transform.position;

		if (playerPosition.x-brushSize <0)
		{
			playerPosition.x = 0+brushSize;
		}
		else if (playerPosition.x + brushSize > size - 1)
		{
			playerPosition.x = size-1 - brushSize;
		}
		if (playerPosition.y - brushSize < 0)
		{
			playerPosition.y = 0 + brushSize;
		}
		else if (playerPosition.y + brushSize > size - 1)
		{
			playerPosition.y = size-1 - brushSize;
		}
		if (playerPosition.z - brushSize < 0)
		{
			playerPosition.z = 0 + brushSize;
		}
		else if (playerPosition.z + brushSize > size - 1)
		{
			playerPosition.z = size - 1 - brushSize;
		}

		//Debug.Log(playerPosition);
		for(int x= (int)playerPosition.x-brushSize;x<= (int)playerPosition.x + brushSize;x++)
		{
			for (int y = (int)playerPosition.y - brushSize; y <= (int)playerPosition.y + brushSize; y++)
			{
				for (int z = (int)playerPosition.z - brushSize; z <= (int)playerPosition.z + brushSize; z++)
				{
					if (Vector3.Distance(playerPosition, new Vector3(x, y, z)) <= brushSize)
					{
						data[x, y, z] = 1.0f;
					}
				}

			}

		}

		previousPosition = playerPosition;

	}

	void FillData(float xOrigin, float yOrigin, float zOrigin) {
		for (int x = 0; x < size; x++) {
			for (int y = 0; y < size; y++) {
				for (int z = 0; z < size; z++)
				{
					//Make all the outside edges solid, by wrapping the solids in -1s
					if(x == 0 || x == size-1) {
						data[x,y,z] = -1;
						continue;
					}
					if(y == 0 || y == size-1) {
						data[x,y,z] = -1;
						continue;
					}
					if(z == 0 || z == size-1) {
						data[x,y,z] = -1;
						continue;
					}

					float dataX = (xOrigin + x)/noiseScaleFactor;
					float dataY = (yOrigin + y)/noiseScaleFactor;
					float dataZ = (zOrigin + z)/noiseScaleFactor;

					//Use the built in Perlin noise to generate some passable noise data.

					//data[x,y,z] = Mathf.PerlinNoise(dataY,dataX+dataZ) - Mathf.PerlinNoise(dataX,dataZ);
					data[x, y, z] = -1;

					//Apply a gradient so our values are more likely to be:
					// "air" (less than 0) at the top and "solid" (greater than 0) at the bottom
					//data[x,y,z] += -(((float)y/size)-.5f);

				}
			}
		}

		//Set some data points manually just to see them displayed.
		data[12,20,12] = .2f;
		data[13,20,12] = 1;
		//data[14,20,12] = .2f;
		//data[15, 20, 12] = .5f;
		//data[15, 20, 11] = .9f;
	}

}
