using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectTagger {

	Dictionary<GameObject, TaggedObject> taggedObjects;

	public class TaggedObject {

		public GameObject target;
		public Vector3 offset;
		public bool clampToScreen;
		public float screenEdgePadding = 5;
		GUIStyle style;
		GUIContentGenerator contentGenerator;

		//Now with generator included with each object
		public TaggedObject(GameObject target, bool clampToScreen, Vector3 offset,
		                    GUIStyle style, GUIContentGenerator contentGenerator) {
			this.target = target;
			this.clampToScreen = clampToScreen;
			this.offset = offset;
			this.style = style;
			this.contentGenerator = contentGenerator;
		}

		public TaggedObject(GameObject target, bool clampToScreen, Vector3 offset, GUIStyle style) {
			this.target = target;
			this.clampToScreen = clampToScreen;
			this.offset = offset;
			this.style = style;
		}

		//Draw method for drawing GUI content over the target object
		public virtual Rect Draw() {
			if(contentGenerator == null) {
				Debug.LogError("Content generator must be defined to use this draw call");
			}
			GUIContent content = contentGenerator(this);
			//Get the position of the object (plus offset) on screen and convert it to Screen coordinates
			Vector3 position = Camera.main.WorldToScreenPoint(target.transform.position + offset);
			position.y = Screen.height - position.y;
			
			//Get the size of the GUIContent
			Vector2 tagSize = style.CalcSize(content);
			//Center the tag over the position
			
			Vector2 newPosition = new Vector2(position.x - (tagSize.x/2), position.y - (tagSize.y/2));
			
			//Properly place the rectangle for drawing on screen (or off screen?)
			Rect contentRectangle = new Rect(newPosition.x, newPosition.y, tagSize.x, tagSize.y);
			if(clampToScreen) {
				contentRectangle = GUIUtils.PlaceRectangleOnScreen(contentRectangle, screenEdgePadding);
			}
			
			GUI.Box(contentRectangle, content, style);
			return contentRectangle;
		}

		//Draw method for drawing GUI content over the target object
		public virtual Rect Draw(GUIContent content) {
			//Get the position of the object (plus offset) on screen and convert it to Screen coordinates
			Vector3 position=new Vector3();
			if (target != null) {
				position = Camera.main.WorldToScreenPoint (target.transform.position + offset);
			}
			position.y = Screen.height - position.y;

			//Get the size of the GUIContent
			Vector2 tagSize = style.CalcSize(content);
			//Center the tag over the position

			Vector2 newPosition = new Vector2(position.x - (tagSize.x/2), position.y - (tagSize.y/2));

			//Properly place the rectangle for drawing on screen (or off screen?)
			Rect contentRectangle = new Rect(newPosition.x, newPosition.y, tagSize.x, tagSize.y);
			if(clampToScreen) {
				contentRectangle = GUIUtils.PlaceRectangleOnScreen(contentRectangle, screenEdgePadding);
			}

			GUI.Box(contentRectangle, content, style);
			return contentRectangle;
		}

		//A more advanced Draw call for passing in whatever kind of content we want,
		// more work than using GUIContent however
		public virtual Rect Draw(GUIElementGetSize getSize, GUIElementDraw elementDrawer) {
			//Get the position of the object (plus offset) on screen and convert it to Screen coordinates
			Vector3 position = Camera.main.WorldToScreenPoint(target.transform.position + offset);
			position.y = Screen.height - position.y;

			//Get the size of the custom element
			Vector3 tagSize = getSize(this, style);
			//Center the tag over the position
			Vector2 newPosition = new Vector2(position.x - (tagSize.x/2), position.y - (tagSize.y/2));

			//Properly place the rectangle for drawing on screen (or off screen?)
			Rect contentRectangle = new Rect(newPosition.x, newPosition.y, tagSize.x, tagSize.y);
			if(clampToScreen) {
				contentRectangle = GUIUtils.PlaceRectangleOnScreen(contentRectangle, screenEdgePadding);
			}

			//Draw the custom element
			elementDrawer(contentRectangle, this, style);
			return contentRectangle;
		}
	}

	public class FancyTaggedObject : TaggedObject {
		Vector2 lineSourcePosition;
		Texture2D lineTexture;

		public FancyTaggedObject(GameObject target, bool clampToScreen,
		                         Vector3 offset, GUIStyle style, Vector2 source,
		                         Texture2D lineTexture)
		: base(target, clampToScreen, offset, style) {
			this.lineSourcePosition = source;
			this.lineTexture = lineTexture;
		}

		public override Rect Draw(GUIContent content)
		{
			Rect labelRectangle = base.Draw (content);

			Vector2 nearestCorner = GUIUtils.NearestPointOnPerimeter(labelRectangle, lineSourcePosition);

			GUIUtils.DrawLine(lineSourcePosition, nearestCorner, Color.black, 3, lineTexture);

			return labelRectangle;
		}
	}


	public ObjectTagger() {
		taggedObjects = new Dictionary<GameObject, TaggedObject>();
	}
	
	public TaggedObject Add(GameObject objectToTag, GUIStyle style, Vector3 offset) {
		return Add (objectToTag, style, offset, true, 5f, null);
	}

	//Now with generator included with each object
	public TaggedObject Add(GameObject objectToTag, GUIStyle style, Vector3 offset,
	                        GUIContentGenerator generator) {
		return Add (objectToTag, style, offset, true, 5f, generator);
	}

	//Now with generator included with each object
	public TaggedObject Add(GameObject objectToTag, GUIStyle style, Vector3 offset,
	                        bool clampToScreen, float screenEdgePadding, GUIContentGenerator generator) {
		TaggedObject newTaggedObject = new TaggedObject(objectToTag, clampToScreen, offset, style, generator);
		taggedObjects.Add(objectToTag, newTaggedObject);
		return newTaggedObject;
	}

	public TaggedObject AddFancy(GameObject objectToTag, GUIStyle style, Vector2 source, Vector3 offset) {
		return AddFancy(objectToTag, style, source, null, offset, true, 5f);
	}

	public TaggedObject AddFancy(GameObject objectToTag, GUIStyle style, Vector2 source,
	                             Texture2D lineTexture, Vector3 offset, bool clampToScreen,
	                             float screenEdgePadding) {

		FancyTaggedObject newTaggedObject = 
			new FancyTaggedObject(objectToTag, clampToScreen, offset, style, source, lineTexture);
		taggedObjects.Add(objectToTag, newTaggedObject);
		return newTaggedObject;
	}

	public void Remove(GameObject objectToRemove) {
		if(taggedObjects.ContainsKey(objectToRemove))
			taggedObjects.Remove(objectToRemove);
	}

	//Simple GUIContent generator. To be run on each tagged object to output GUIContent
	public delegate GUIContent GUIContentGenerator(TaggedObject taggedObject);


	//Simple GUIContent drawer
	public void Draw(GUIContentGenerator contentGenerator) {
		foreach(TaggedObject taggedObject in taggedObjects.Values) {
			GUIContent content = contentGenerator(taggedObject);
			taggedObject.Draw(content);
		}
	}

	//Simple GUIContent drawer
	public void Draw() {
		foreach(TaggedObject taggedObject in taggedObjects.Values) {
			//Ensure the gameobject hasn't been removed from the world.
			//We should remove tagging if we're removing an object
			if(taggedObject.target != null)
				taggedObject.Draw();
		}
	}


	//Custom drawer for each object. Paired with the GetSize, this can be passed in to draw whatever we like 
	public delegate void GUIElementDraw(Rect contentRectangle, TaggedObject taggedObject, GUIStyle style);

	//Gets the size of the custom object before drawing.
	public delegate Vector2 GUIElementGetSize(TaggedObject taggedObject, GUIStyle style);
	

	public void Draw(GUIElementGetSize getSize, GUIElementDraw elementDrawer) {
		foreach(TaggedObject taggedObject in taggedObjects.Values) {
			taggedObject.Draw(getSize, elementDrawer);
		}
	}

}
