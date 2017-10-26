using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentCreator : MonoBehaviour {


	[SerializeField]
	private GameObject agent;


	[SerializeField]
	private float rangeX = 100.0f;

	[SerializeField]
	private float rangeY = 100.0f;

	[SerializeField]
	private float rangeZ = 1.0f;



	//floating script
	private float movementDistance = 1.0f; // The maximum distance can move up and down
	private float range;


	//pod custom prefab
	private Dictionary<string, CustomPrefab1> agents = new Dictionary<string, CustomPrefab1>();


	// Use this for initialization
	void Start () {
		randomCreator (rangeX,rangeY,rangeZ);
	
	}
	
	// Update is called once per frame
	void Update () {
		//check if layer finished
		for (int i = 0; i < SwarmManager.Instance.layersNumber; i++) {
			//if finished
			if (SwarmManager.Instance.nextAgent [i] == -2) {
				for (int j = 0; j < SwarmManager.Instance.agentsNumber; j++) {
					//if not locked
					if (SwarmManager.Instance.locked [i] [j] == false) {
						//delete
						destroyPrefab(agents,"Agents",j,i);
					}
					
				}
			}
		}
		
	}

	private void createPrefab(Dictionary<string, CustomPrefab1> prefabs, int c, Vector3 p, Vector3 s, string t,int k, GameObject origin)
	{
		string prefabName=t+k+""+c;
		prefabs.Add(prefabName, new CustomPrefab1(prefabName, p,s,t,origin));
		prefabs[prefabName].Instantiate();
		prefabs [prefabName].access ().transform.parent = this.gameObject.transform;
	}

	private void destroyPrefab(Dictionary<string, CustomPrefab1> prefabs,string t,int c,int k)
	{
		string prefabName=t+k+""+c;
		//for(int i=0;i <prefabs.Count;i++)
		if(prefabs.ContainsKey(prefabName))
		{
			Destroy(GameObject.Find(prefabName));
			prefabs [prefabName].delete ();

		}
		//prefabs.Clear ();
	}
	private void randomCreator (float rangeX,float rangeY, float rangeZ)
	{
		int currentCount=0;
		while (currentCount < SwarmManager.Instance.agentsNumber) {



			for (int k = 0; k < SwarmManager.Instance.layersNumber; k++) {

				float newY =  currentCount % 10;
				float newX= currentCount/10;
				float newZ=k;

				Vector3 tempScale=new Vector3 (1.0f, 1.0f, 1.0f);
				Vector3 tempVec = new Vector3 (newX, newY, newZ);



				string tag = "Agents";
				createPrefab (agents, currentCount, tempVec, tempScale, tag,k, agent);
			}
			currentCount++;
		}

		
	}
}
