using UnityEngine;
using System.Collections;

public class CustomPrefab {

	string name;
	string[] dataLines;
	int dataPointer = 0;

	public CustomPrefab(string name, string[] scriptLines) {
		this.name = name;
		this.dataLines = scriptLines;
	}

	public GameObject Instantiate() {
		//Create an empty game object to work with
		//in this example we'll create a sphere (so we have something to look at in game)
		//however, you can easily define a custom mesh of your own.
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		go.name = name;
		while(dataPointer < dataLines.Length) {
			if(dataLines[dataPointer].Length < 1) {
				dataPointer++;//skip any whitespace
				continue;
			}

			if(dataLines[dataPointer].StartsWith("customcomponent:")) {
				//Separate the component name from the header
				string componentName = dataLines[dataPointer].Substring(dataLines[dataPointer].IndexOf(":") + 1);
				dataPointer++;
				//Unity uses C# reflection and allows us to simply pass the string name of the script we want to add
				CustomComponentBase c = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(go, "Assets/Chapter 3/BasicSystem/CustomPrefab.cs (32,29)", componentName) as CustomComponentBase;
                //CustomComponentBase c = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(go, "Assets/Chapter 3/BasicSystem/CustomPrefab.cs (32,29)", componentName) as CustomComponentBase;

                //This is similar to calling go.AddComponent("ComponentA");
                while (dataLines[dataPointer].Length < 1) {
					dataPointer++;//clear any white space after the component token
				}

				if(c != null) {
					//special components we want to add should implement the ComponentType class, this way we can call
					// our special SetData function
					//pass the dataPointer as a references (ref) so that we'll continue with any modifcations when we get back here

					c.SetData(dataLines, ref dataPointer);
				} else {
					Debug.Log("Error adding " + componentName + "! Ensure the name is typed correctly.");
				}
			} else if (dataLines[dataPointer].StartsWith("position:")) {
				string vec3Position = dataLines[dataPointer].Substring(dataLines[dataPointer].IndexOf(":") + 1);
				string[] posComponents = vec3Position.Split(',');
				go.transform.position = new Vector3(float.Parse(posComponents[0]), float.Parse(posComponents[1]), float.Parse(posComponents[2]));
				dataPointer++;
				continue;
			// else if (other keywords?) {
				//other processing
			//}
			} else {
				Debug.Log("Line: `" + dataLines[dataPointer] + "` not recognized as valid token");
				dataPointer++;
				continue;
			}
		}
		dataPointer = 0;
		return go;
	}
    
}
