using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour {
	public Camera leftCamera;
	public Camera rightCamera;

	public void ShowLeftView(){
		rightCamera.enabled = false;
		leftCamera.enabled = true;
	}

	public void ShowRightView(){
		leftCamera.enabled = false;
		rightCamera.enabled = true;
	}
}
