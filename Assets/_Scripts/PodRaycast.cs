using UnityEngine;
using System.Collections;

public class PodRaycast : MonoBehaviour {


	private RaycastHit hit=new RaycastHit();

	private Behaviour halo;
	private GameObject temp;
	private Rect screenRect;
	private Ray ray;
	private bool wasHit=false;

	public bool thisHit = false;
	public bool clicked = false;
	public bool playVideo =false;


	private bool alreadyHit = false;

	// Use this for initialization
	void Start () {
		temp = this.gameObject.transform.GetChild(0).gameObject;
		halo =(Behaviour)temp.gameObject.GetComponent ("Halo");
		screenRect = new Rect(0,0, Screen.width, Screen.height);

		//StartCoroutine (raycast());
	}

	// Update is called once per frame
	void Update () {
		ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		wasHit = Physics.Raycast (ray, out hit, 200);

		if (wasHit && hit.collider.gameObject == this.gameObject) {
			if (Input.GetMouseButtonDown (0)) {
				clicked = true;
				Debug.Log ("Pod was clicked.");
			}
			if (!alreadyHit) {
				//Debug.Log ("Pod was hit.");

				thisHit = true;
				alreadyHit= true;
			}
			//halo.enabled = false;
		} 
		else 
		{
			thisHit = false;
			alreadyHit= false;
			clicked = false;
			//halo.enabled = true;
		}

		
	}
	private IEnumerator raycast()
	{
		while (true) {
			//yield return new WaitForSeconds (1f);
			yield return 0;
		}
	}
}
