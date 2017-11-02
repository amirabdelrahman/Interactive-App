using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraShaderReplace : MonoBehaviour {

    public Shader replacementShader;
    public RenderTexture RTargetTexture;
	// Use this for initialization
	void Start () {
		GetComponent<Camera>().SetReplacementShader(replacementShader, "");
    }
	
	// Update is called once per frame
	void Update () {
        if (GetComponent<Camera>().targetTexture != RTargetTexture)
             GetComponent<Camera>().targetTexture = RTargetTexture;

    }
}
