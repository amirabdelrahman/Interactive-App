using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class DynamicMeshGenerator {
	
	public static Mesh GenerateMesh(List<Vector3> vertices) {
		List<int> newTriangles = new List<int>();

		//We'll support both quads and triangles. 
		//If the number of vertices is divisible by 4, try to make quads
		if(vertices.Count %4 == 0) {
			for(int vertexPointer = 0; vertexPointer + 4 <= vertices.Count; vertexPointer += 4){
				newTriangles.Add(vertexPointer+0); //0
				newTriangles.Add(vertexPointer+1); //1
				newTriangles.Add(vertexPointer+2); //2

				newTriangles.Add(vertexPointer+0); //0
				newTriangles.Add(vertexPointer+2); //2
				newTriangles.Add(vertexPointer+3); //3
			}
		} else { //Otherwise, make as many triangles as we can
			for(int vertexPointer = 0; vertexPointer + 3 <= vertices.Count; vertexPointer += 3){
				newTriangles.Add(vertexPointer+0); //0
				newTriangles.Add(vertexPointer+1); //1
				newTriangles.Add(vertexPointer+2); //2
			}
		}

		List<Vector2> texCoords = new List<Vector2>();
		Vector2 emptyTexCoords = new Vector2(0,0);
		for(int texturePointer = 0; texturePointer < vertices.Count; texturePointer++) {
			//There should be as many texture coordinates as vertices.
			//This example does not support textures, so fill with zeros
			texCoords.Add(emptyTexCoords);
		}

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.uv = texCoords.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}
}
