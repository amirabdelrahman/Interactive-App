using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Draggable : MonoBehaviour {


    private Vector3 screenPoint;
    private Vector3 offset;
    private float originalY;


    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        originalY = this.transform.position.y;
        //Debug.Log(this.gameObject.name.Remove(0, 7));
		//ImmersiveSimulationManager.Instance.changed = true;
        ImmersiveSimulationManager.Instance.controlPointChanged = Convert.ToInt32(this.gameObject.name.Remove(0, 7));
        //Debug.Log(ImmersiveSimulationManager.Instance.controlPointChanged);
    }

    void OnMouseDrag()
    {
        
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        //transform.position = curPosition;
        transform.position = new Vector3(this.transform.position.x, curPosition.y, this.transform.position.z);
        ImmersiveSimulationManager.Instance.heightValueChanged = originalY - curPosition.y;
    }
    private void OnMouseUp()
    {
        ImmersiveSimulationManager.Instance.changed = true;
    }
}
