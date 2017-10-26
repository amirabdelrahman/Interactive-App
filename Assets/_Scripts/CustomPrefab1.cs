using UnityEngine;
using System.Collections;

public class CustomPrefab1
{

    string name;
    Vector3 positon;
    Vector3 scale;
	string tag;
	GameObject go;
	GameObject origin;
    
	public CustomPrefab1(string name, Vector3 positon, Vector3 scale, string tag)
    {
        this.name = name;
        this.positon = positon;
		this.scale = scale;
		this.tag = tag;
    }
	public CustomPrefab1(string name, Vector3 positon, Vector3 scale, string tag,GameObject origin)
	{
		this.name = name;
		this.positon = positon;
		this.scale = scale;
		this.tag = tag;
		this.origin = origin;
	}

    public GameObject Instantiate()
    {
        //Create an empty game object to work with
        //in this example we'll create a sphere (so we have something to look at in game)
        //however, you can easily define a custom mesh of your own.
		//GameObject go= GameObject.CreatePrimitive (PrimitiveType.Cube);


		if (this.tag == "Suggestion") {
			//this.go=GameObject.Instantiate(
			this.go = GameObject.CreatePrimitive (PrimitiveType.Cube);
			this.go.name = name;
			this.go.tag = tag;
			this.go.transform.position = this.positon;
			this.go.transform.localScale = this.scale;

		/*} else if (this.tag == "Suggestion") {
			this.go = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
			this.go.name = name;
			this.go.tag = tag;
			this.go.transform.position = this.positon;
			this.go.transform.localScale = this.scale;*/

		} else if(this.tag == "Nodes" || this.tag=="Control")
        {
			//this.go =(GameObject)Instantiate(Resources.Load("Pod"))as GameObject; 
			this.go =GameObject.Instantiate(origin)as GameObject;
			this.go.name = this.name;
			this.go.tag = this.tag;
			this.go.transform.position = this.positon;
			this.go.transform.localScale = this.scale;
		}
        else {
			this.go = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
            this.go.name = name;
			this.go.tag = tag;
			this.go.transform.position = this.positon;
			this.go.transform.localScale = this.scale;
		}
		return this.go;
    }

	public void delete()
	{
		GameObject.Destroy (go);
	}

	public GameObject access()
	{
		return go;
	}


}
