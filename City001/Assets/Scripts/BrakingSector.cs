using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrakingSector : MonoBehaviour {

	public float maxBrakeTorque = 5000;
	public float minCarSpeed = 0;


	public void OnTriggerStay(Collider other){

		if (other.CompareTag("Car")) {
			
			float currentSpeedControl = other.transform.GetComponentInParent<CarEngine>().currentSpeed;

			if (currentSpeedControl >= minCarSpeed) {
				if (currentSpeedControl >= 10) {
					other.transform.GetComponentInParent<CarEngine> ().wheelRL.brakeTorque = 4f * maxBrakeTorque;
					other.transform.GetComponentInParent<CarEngine> ().wheelRR.brakeTorque = 4f * maxBrakeTorque;
					other.transform.GetComponentInParent<CarEngine> ().wheelFL.motorTorque = 0;
					other.transform.GetComponentInParent<CarEngine> ().wheelFR.motorTorque = 0;
					other.transform.GetComponentInParent<CarEngine>().isBraking = true;

				} else {
						if (currentSpeedControl < 10 && currentSpeedControl >= 5) {
							other.transform.GetComponentInParent<CarEngine> ().wheelRL.brakeTorque = 3f * maxBrakeTorque; 
							other.transform.GetComponentInParent<CarEngine>().wheelRR.brakeTorque = 3f * maxBrakeTorque;
							other.transform.GetComponentInParent<CarEngine> ().wheelFL.motorTorque = 0;
							other.transform.GetComponentInParent<CarEngine> ().wheelFR.motorTorque = 0;
							other.transform.GetComponentInParent<CarEngine>().isBraking = true;
					} else {
							if (currentSpeedControl < 5) {
							other.transform.GetComponentInParent<CarEngine> ().wheelRL.brakeTorque =maxBrakeTorque;
							other.transform.GetComponentInParent<CarEngine> ().wheelRR.brakeTorque = maxBrakeTorque;
							other.transform.GetComponentInParent<CarEngine> ().wheelFL.motorTorque = 0;
							other.transform.GetComponentInParent<CarEngine> ().wheelFR.motorTorque = 0;
							other.transform.GetComponentInParent<CarEngine>().isBraking = true;
							}
						}
					}
			} else {
				other.transform.GetComponentInParent<CarEngine>().wheelRL.brakeTorque = 0;
				other.transform.GetComponentInParent<CarEngine>().wheelRR.brakeTorque = 0;
			}
		}
	}

	public void OnTriggerExit(Collider other){
		if (other.CompareTag("Car")){
			other.transform.GetComponentInParent<CarEngine>().isBraking = false;
			other.transform.GetComponentInParent<CarEngine>().wheelRL.brakeTorque = 0;
			other.transform.GetComponentInParent<CarEngine>().wheelRR.brakeTorque = 0;
		}
	}
}
