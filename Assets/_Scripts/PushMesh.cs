using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushMesh : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey(KeyCode.Q))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                Debug.Log("here");
                Debug.Log(hitInfo.point);
                //AddDensity(hitInfo.point, toolRadius);
            }
        }


    }
}
