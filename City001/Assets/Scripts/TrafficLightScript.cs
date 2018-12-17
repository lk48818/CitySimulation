using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightScript : MonoBehaviour {

	public Transform trafficLights;
	public Transform redLightBlockFirst;
	public Transform redLightBlockSecond;

	private List<Light> trafficLight = new List<Light>();
	private bool isBlockSet = false;
	private Vector3 changePosition = new Vector3 (0, 3, 0);

	void Start () {
		Light[] lights = trafficLights.GetComponentsInChildren<Light> ();
		trafficLight = new List<Light> ();

		for(int i = 0; i< lights.Length;i++){
			if (lights [i] != trafficLights.transform)
				trafficLight.Add (lights [i]);
		}
	}

	void FixedUpdate () {
		LightChanges ();
	}

	void LightChanges(){
		
		if (trafficLight [1].GetComponent<Light> ().enabled && !isBlockSet) {
			//Debug.Log ("red light");
			isBlockSet = true;
			redLightBlockFirst.transform.Translate(changePosition);
			redLightBlockSecond.transform.Translate(changePosition);
		}

		//isBlockSet = false;
		if (trafficLight [0].GetComponent<Light> ().enabled && isBlockSet) {
			//Debug.Log ("green light");
			isBlockSet = false;	
			redLightBlockFirst.transform.Translate (-changePosition);
			redLightBlockSecond.transform.Translate (-changePosition);
			}
		
	}
}
