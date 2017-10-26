using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImmersiveSimulationManager : Singleton<ImmersiveSimulationManager>
{
    /// private functions///
	private bool _changed=false;
    private int _controlPointChanged;
    private float _heightValueChanged;


    /// public Functions ///
    public bool changed
    {
        get { return _changed; }
        set { _changed = value; }
    }
    public int controlPointChanged
    {
        get { return _controlPointChanged; }
        set { _controlPointChanged = value; }
    }
    public float heightValueChanged
    {
        get { return _heightValueChanged; }
        set { _heightValueChanged = value; }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
