using UnityEngine;
using System.Collections;

public class CastRay : MonoBehaviour {

	private RaycastHit hit;

    float depthIntoScene = 10;

    float defaultDepthIntoScene = 5;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        //RaycastHit hit;
        float depth;

		Ray ray = Camera.current.ScreenPointToRay (Input.mousePosition);
		bool wasHit = Physics.Raycast (ray, out hit);
        
		if (wasHit && hit.collider.gameObject == this.gameObject) {
			//Debug.Log ("Plane was hit.");
            NeuralManager.Instance.point = hit.point;
		} else {
            //Debug.Log ("Plane wasn't hit.");  
            //if we didn't hit anything, set the depth to the arbitrary depth
            //depth = depthIntoScene;
            //now we can reuse our previous code to position the object using the depth we defined here
            //MoveToMouseAtSpecifiedDepth(depth);
        }
	}

	void OnDrawGizmos() {
		
		Gizmos.DrawSphere (hit.point, 3.0f);
	}

    void MoveToMouseAtSpecifiedDepth(float depth)
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = depth;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        this.transform.position = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, mouseWorldPosition.z);
    }
}
