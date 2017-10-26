using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.ComponentModel;
using System.Threading;

public class NeuralManager : Singleton<NeuralManager> {

	private float _timeRemaining;

	public Vector3 point = new Vector3(50.0f,0.0f,50.0f);




    public float TimeRemaining 
	{
		get { return _timeRemaining; }
		set { _timeRemaining = value; }
	}

	private float maxTime = 10; // In seconds.

	private int frameCount = 0;

	// Use this for initialization
	void Start () {
		TimeRemaining = maxTime;	
	}
	
	// Update is called once per frame
	void Update () {
		TimeRemaining -= Time.deltaTime;

		if (TimeRemaining <= 0)
		{
			//Application.LoadLevel(Application.loadedLevel);
			TimeRemaining = maxTime;
		}	

		//Debug.Log (this.point);
		//Application.CaptureScreenshot ("/Users/nono/Desktop/Screens/ScreeScreenshot_" + frameCount.ToString("00000") + ".png");
		frameCount++;
	}

}
