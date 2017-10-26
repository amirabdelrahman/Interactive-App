using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AgentTagger : MonoBehaviour {

	private int currentIndex=0;
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

		temp = new Vector3 (0, 1, 0);

		Debug.Log ("here");

		//currentIndex = Convert.ToInt32 (this.gameObject.name.Remove (0, 6));

	}
	void Update()
	{
		if (!tagged && GameObject.FindGameObjectsWithTag ("Agents").Length > 0) {
			SimpleAdd (temp);
			tagged = true;
		}
		
	}

	void SimpleAdd(Vector3 offset) {

		//Add all the GameObjects with the tag "Tagged" to the tagger list
		foreach(GameObject gameObject in GameObject.FindGameObjectsWithTag("Agents")) {
			tagger.Add(gameObject, taggerSkin.box, offset);
		}
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
		foreach(GameObject gameObject in GameObject.FindGameObjectsWithTag("Agents")) {
			tagger.AddFancy(gameObject, taggerSkin.box, new Vector2(thisScreenPosition.x,
				thisScreenPosition.y), Vector3.zero);
		}
	}

	//Example GUIContentGenerator, outputting the name of the object
	GUIContent GenerateName(ObjectTagger.TaggedObject taggedObject) {
			
		if (taggedObject.target != null) {
			return new GUIContent ("R:" + SwarmManager.Instance.states [Convert.ToInt32 (taggedObject.target.transform.position.z)] [Convert.ToInt32 (taggedObject.target.name.Remove (0, 7))]);

		}
		return new GUIContent ();
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
