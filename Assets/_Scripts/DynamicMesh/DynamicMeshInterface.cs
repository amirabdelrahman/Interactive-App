using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
[AddComponentMenu("Mesh/Dynamic Mesh Generator")]
public class DynamicMeshInterface : MonoBehaviour {

	//Clockwise winding order defines the front of the vertex

	List<Vector3> triangleVertices = new List<Vector3>(){
		new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0)};

	List<Vector3> quadVertices = new List<Vector3>(){
		new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0)};

	List<GameObject> customShape = new List<GameObject>();

	bool regenerateCustom = false;
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown(KeyCode.Alpha1)) {
			ApplyMesh(triangleVertices);
		}

		if(Input.GetKeyDown(KeyCode.Alpha2)) {
			ApplyMesh(quadVertices);
		}

		if(Input.GetKeyDown(KeyCode.Alpha3)) {
			AddPointToScene(MouseUtils.GetMouseWorldPositionAtDepth(10));
		}

		if(Input.GetKeyDown(KeyCode.Alpha4)) {
			foreach(GameObject point in customShape)
				GameObject.Destroy(point);
			customShape.Clear();
			regenerateCustom = true;
		}

		if(Input.GetMouseButton((int)MouseUtils.Button.Left)) {
			regenerateCustom = true;
		}

		if(regenerateCustom) {
			List<Vector3> customPoints = new List<Vector3>();
			foreach(GameObject go in customShape) {
				customPoints.Add(go.transform.position);
			}
			ApplyMesh(customPoints);
			regenerateCustom = false;
		}
	}

	void ApplyMesh(List<Vector3> vertices) {
		Mesh mesh = DynamicMeshGenerator.GenerateMesh(vertices);
		GetComponent<MeshFilter>().mesh = mesh;
	}

	void AddPointToScene(Vector3 point) {
		GameObject newPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		newPoint.transform.position = point;
		newPoint.transform.localScale = new Vector3(.3f, .3f, .3f);
		MouseDragObject mdo = newPoint.AddComponent<MouseDragObject>();
		customShape.Add(newPoint);
		regenerateCustom = true;
	}

}
