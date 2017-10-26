using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class SwarmManager : Singleton<SwarmManager> {


	/// private functions///
	private List<bool> _agentMoving =new List<bool>();

	//-1 initialized //-2 stopped/reached
	private List<int> _nextAgent=new List<int>();

	//temp list to update the mesh
	private List<Vector3> _fillAgent=new List<Vector3>();

	[SerializeField]
	private int _agentsNumber = 100;

	private int _layersNumber=10;

	[SerializeField]
	private List<int> _currentMoving = new List<int>();

	private List<List<int>> _states = new List<List<int>> ();


	//-2 if stationary, same position if start
	private List<List<List<int>>> _target = new List<List<List<int>>> ();

	private List<List<List<int>>> _edges = new List<List<List<int>>> ();

	private List<List<bool>> _locked = new List<List<bool>> ();

	///////////////public functions////
	public List<bool> agentMoving 
	{
		get { return _agentMoving; }
		set { _agentMoving = value; }
	}
	public int agentsNumber 
	{
		get { return _agentsNumber; }
		set { _agentsNumber = value; }
	}
	public int layersNumber 
	{
		get { return _layersNumber; }
		set { _layersNumber = value; }
	}
	public List<int> nextAgent 
	{
		get { return _nextAgent; }
		set { _nextAgent= value; }
	}
	public List<Vector3> fillAgent 
	{
		get { return _fillAgent; }
		set { _fillAgent= value; }
	}
	public List<int> currentMoving 
	{
		get { return _currentMoving; }
		set { _currentMoving = value; }
	}
	public List<List<int>> states 
	{
		get { return _states; }
		set { _states = value; }
	}
	public List<List<List<int>>> target 
	{
		get { return _target; }
		set { _target = value; }
	}
	public List<List<List<int>>> edges 
	{
		get { return _edges; }
		set { _edges = value; }
	}
	public List<List<bool>> locked 
	{
		get { return _locked; }
		set { _locked = value; }
	}




	// Use this for initialization
	void Start () {
		List<int> tempState = new List<int> ();

		List<bool> tempLocked = new List<bool> ();

		//initialize rankings to 0
		for (int i=0;i<agentsNumber;i++)
		{
			tempState.Add(0);
			//target object and direction
			tempLocked.Add (false);
		}

		//initialize the first agent to be at ranking 1
		tempState [0] = 1;

		//initialize 
		for (int j = 0; j < _layersNumber; j++) {
			nextAgent.Add (-1);
			agentMoving.Add (false);
			currentMoving.Add (0);
			states.Add (new List<int>(tempState));
			locked.Add (new List<bool>(tempLocked));

			List<List<int>> tempTarget = new List<List<int>> ();
			List<List<int>> tempEdge = new List<List<int>> ();

			for (int i = 0; i < agentsNumber; i++) {
				
				List<int> tempArray = new List<int>();
				tempArray.Add (i);
				tempArray.Add (-1);


				tempTarget.Add (new List<int>(tempArray));

				//initialize edges
				List<int> tempNoEdge = new List<int> ();
				// 0 north (+x), 2 south (-x), 1 east (+z), 3 west (-z)
				for (int k = 0; k < 4; k++) {
					tempNoEdge.Add (-1);
				}

				tempEdge.Add (new List<int> (tempNoEdge));
			}
			target.Add (new List<List<int>> (tempTarget));
			edges.Add (new List<List<int>> (tempEdge));
		}


	}
	
	// Update is called once per frame
	void Update () {
		
		for (int i = 0; i < layersNumber; i++) 
		{
			if (readyToMove(i)) 
			{
				if (!_agentMoving [i]) 
				{
					updateHighestRank (i);
				}
			}
		}
	
	}
	private bool readyToMove (int i)
	{
		foreach (int n in states[i]) {
			if (n == 0)
				return false;
		}
		return true;
	}

	private void updateHighestRank(int k)
	{
		int maximum = -1;
		int maximumIndex = -1;
		int i = 0;
		foreach (int n in states[k]) {
			if (n >= maximum && !locked[k][i]) {
				maximum = n;
				maximumIndex = i;
			}
			i++;
		}
		nextAgent[k] = maximumIndex;
	}

}
