using UnityEngine;
using System.Collections;

public class PodTagger : MonoBehaviour {

	[SerializeField]
	private PodRaycast _PodRaycast;

	ObjectTagger tagger;

	[SerializeField]
	private GUISkin taggerSkin;
	Texture2D target;

	private bool tagged = false;

	private Vector3 temp;
	// Use this for initialization
	void Start () {
		tagger = new ObjectTagger();
		target = Resources.Load("Target") as Texture2D;

		temp = new Vector3 (0, 2, 0);
		//SimpleAdd(Vector3.zero);
		//FancyAdd();
	}
	void Update()
	{
		if (_PodRaycast.thisHit ) {
			if(!tagged)
			{
				
				//SimpleAdd (Vector3.forward);
				SimpleAdd (temp);
				//Debug.Log(this.gameObject.name);
				tagged = true;
			}
		} else {
			Remove ();
			tagged = false;
		}
	}

	void SimpleAdd(Vector3 offset) {
		
		//Add all the GameObjects with the tag "Tagged" to the tagger list
		//foreach(GameObject gameObject in GameObject.FindGameObjectsWithTag("Tagged")) {
			tagger.Add(this.gameObject, taggerSkin.box, offset);
		//}
	}
	void Remove()
	{
		tagger.Remove (this.gameObject);
	}



	void FancyAdd() {
		//Get the on-screen position of ourself, to pass as the source point for drawing a line
		// from us to the target
		Vector3 thisScreenPosition = Camera.main.WorldToScreenPoint(this.transform.position);
		thisScreenPosition.y = Screen.height - thisScreenPosition.y;

		//Add all the GameObjects with the tag "Tagged" to the tagger list
		foreach(GameObject gameObject in GameObject.FindGameObjectsWithTag("Tagged")) {
			tagger.AddFancy(gameObject, taggerSkin.box, new Vector2(thisScreenPosition.x,
				thisScreenPosition.y), Vector3.zero);
		}
	}

	//Example GUIContentGenerator, outputting the name of the object
	GUIContent GenerateName(ObjectTagger.TaggedObject taggedObject) {
		if (this.tag == "PodGreen") {
			return new GUIContent("Tangible Exhibit");
		} else if (this.tag == "PodRed") {
			return new GUIContent("Visualization Exhibit");
		} else if (this.tag == "PodBlue") {
			return new GUIContent("Web Exhibit");
		} else {
			return new GUIContent("Immersive Exhibit");
		}
		//return new GUIContent(taggedObject.target.name);
	}

	//Example GUIContentGenerator, outputting the name and position of the object
	GUIContent GenerateSimpleInfo(ObjectTagger.TaggedObject taggedObject) {
		return new GUIContent(taggedObject.target.name + "\n" +
			taggedObject.target.transform.position);
	}

	//Example GUIContentGenerator, creating a target texture
	GUIContent GenerateTarget(ObjectTagger.TaggedObject taggedObject) {
		return new GUIContent(target);
	}

	//Custom drawing example of drawing an element
	void GUIElementDraw(Rect contentRectangle, ObjectTagger.TaggedObject taggedObject, GUIStyle style) {
		GUI.Label(contentRectangle, new GUIContent(target));
	}

	//Custom drawing example of getting size
	Vector2 GUIElementGetSize(ObjectTagger.TaggedObject taggedObject, GUIStyle style) {
		return style.CalcSize(new GUIContent(target));
	}

	void OnGUI() {
		tagger.Draw(GenerateName);
		//tagger.Draw(GenerateTarget);
		//tagger.Draw(GUIElementGetSize, GUIElementDraw);
	}
}
