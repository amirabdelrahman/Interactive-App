using UnityEngine;
using System.Collections;

public class TaggingInterface : MonoBehaviour {
	ObjectTagger tagger;
	public GUISkin interfaceSkin;

	// Use this for initialization
	void Start () {
		tagger = new ObjectTagger();
		if(interfaceSkin == null)
			interfaceSkin = new GUISkin();
	}

	public void TagObject(GameObject gameObject, ObjectTagger.GUIContentGenerator contentGenerator,
	                      Vector3 offset) {
		tagger.Add(gameObject, interfaceSkin.box, offset, contentGenerator);
	}

	public void RemoveTag(GameObject gameObject) {
		tagger.Remove(gameObject);
	}

	void OnGUI() {
		tagger.Draw();
	}
}
