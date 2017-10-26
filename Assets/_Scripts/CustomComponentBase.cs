using UnityEngine;
using System.Collections;

public abstract class CustomComponentBase : MonoBehaviour {
	public abstract void SetData(string[] lines, ref int pointer);
}
