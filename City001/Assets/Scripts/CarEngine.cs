using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour {

	public Transform path;
	public WheelCollider wheelFL;
	public WheelCollider wheelFR;
	public WheelCollider wheelRL;
	public WheelCollider wheelRR;
	public Texture2D textureNormal;
	public Texture2D textureBraking; 
	public Renderer carRenderer;

	public Vector3 centerOfMass;

	public float maxSteerAngle = 25.0f;
	public float minWaypointDistance = 0.5f;
	public float minSteerBrakeDistance = 4.0f;
	public float maxMotorTorque = 1000.0f;
	public float maxBrakeTorque = 5000.0f;
	public float maxSpeed = 15.0f;
	public float currentSpeed;
	public float turnSpeed = 5.0f;
	public float carVelocity;

	public bool carBrake = false;
	public bool trafficLightBrake = false;
	public bool isBraking = false;

	public float respawnWait = 10.0f;
	public float respawnCounter =0.0f;

	[Header("Reversing")]
	public bool isReversing = false;
	public float reverseCounter = 0.0f;
	public float waitToReverse = 5.0f;
	public float reverseFor = 2.0f;

	[Header("Sensors")]
	public float sensorLength = 3f;
	public float carSensorLength = 10.0f;
	public float trafficLightSensorLength = 12.0f;
	public Vector3 frontSensorPosition = new Vector3 (0, 0.2f, 1.5f);
	public float frontSideSensorPosition = 0.85f;
	public float frontSensorAngle = 25.0f;

	private List<Transform> nodes;
	private int currentNode = 0;
	private bool avoiding = false;
	private float targetSteerAngle = 0;
	private float steerAngle;

	void Start () {

		GetComponent<Rigidbody> ().centerOfMass = centerOfMass;
		Transform[] pathTransforms = path.GetComponentsInChildren<Transform> ();
		nodes = new List<Transform> ();

		for(int i = 0; i<pathTransforms.Length; i++){
			if (pathTransforms[i] != path.transform)
				nodes.Add (pathTransforms[i]);
		}
	}

	void Update () {
		CarBrakeSensors ();
		TrafficLightsSensors ();
		Sensors ();
		ApplySteer ();
		LerpToSteerAngle ();
		Braking ();
		Drive ();
		CheckWaypointDistance ();
		Respawn ();
		BeforeSteeringBrake ();
	}

	private void TrafficLightsSensors(){
		
		RaycastHit hit;
		Vector3 sensorStartPos = transform.position;
		sensorStartPos += transform.forward * frontSensorPosition.z;
		sensorStartPos += transform.up * (frontSensorPosition.y+0.2f);

		//right traffic light sensor
		sensorStartPos += transform.right * frontSideSensorPosition;
		if (Physics.Raycast (sensorStartPos, transform.forward, out hit, trafficLightSensorLength)) {
			if (hit.collider.CompareTag ("TrafficLightBlock")) {
				Debug.DrawLine (sensorStartPos, hit.point);
				trafficLightBrake = true;
			} else
				trafficLightBrake = false;
		} else
			trafficLightBrake = false;

		//left traffic light sensor
		sensorStartPos -= transform.right * frontSideSensorPosition * 2;
		if (Physics.Raycast (sensorStartPos, transform.forward, out hit, trafficLightSensorLength)) {
			if (hit.collider.CompareTag ("TrafficLightBlock")) {
				Debug.DrawLine (sensorStartPos, hit.point);
				trafficLightBrake = true;
			} else
				trafficLightBrake = false;
		} else
			trafficLightBrake = false;

		//front traffic light sensor
		sensorStartPos += transform.right * frontSideSensorPosition;
		if (Physics.Raycast (sensorStartPos, transform.forward, out hit, trafficLightSensorLength)) {
			if (hit.collider.CompareTag ("TrafficLightBlock")) {
				Debug.DrawLine (sensorStartPos, hit.point);
				trafficLightBrake = true;
			} else
				trafficLightBrake = false;
		} else
			trafficLightBrake = false;
	}

	private void CarBrakeSensors(){
		RaycastHit hit;
		Vector3 sensorStartPos = transform.position;
		sensorStartPos += transform.forward * frontSensorPosition.z;
		sensorStartPos += transform.up * (frontSensorPosition.y+0.1f);

		//car sensor right
		sensorStartPos += transform.right * frontSideSensorPosition;
		if (Physics.Raycast (sensorStartPos, transform.forward, out hit, carSensorLength)) {
			if (hit.collider.CompareTag ("Car")) {
				Debug.DrawLine (sensorStartPos, hit.point);
				if (hit.rigidbody.velocity.magnitude < carVelocity) {
					//maxBrakeTorque = 10000;
					carBrake = true;
				} else 
					//maxBrakeTorque = 5000;
					carBrake = false;
			} else
				carBrake = false;
		} else
			carBrake = false;


		//car sensor left
		sensorStartPos -= transform.right * frontSideSensorPosition * 2;
		if (Physics.Raycast (sensorStartPos, transform.forward, out hit, carSensorLength)) {
			if (hit.collider.CompareTag ("Car")) {
				Debug.DrawLine (sensorStartPos, hit.point);
				if (hit.rigidbody.velocity.magnitude < carVelocity) {
					//maxBrakeTorque = 10000;
					carBrake = true;
				} else
					//maxBrakeTorque = 5000;
					carBrake = false;
			} else 
				carBrake = false;
		} else
			carBrake = false;

		//car sensor front
		sensorStartPos += transform.right * frontSideSensorPosition;
		if (Physics.Raycast (sensorStartPos, transform.forward, out hit, carSensorLength)) {
			if (hit.collider.CompareTag ("Car")) {
				Debug.DrawLine (sensorStartPos, hit.point);
				if (hit.rigidbody.velocity.magnitude < carVelocity) {
					//maxBrakeTorque = 10000;
					carBrake = true;
					//}
				} else
					//maxBrakeTorque = 5000;
					carBrake = false;
			} else 
				carBrake = false;
		} else
			carBrake = false; 
	}

	private void Sensors(){
		
		RaycastHit hit;
		float avoidMultiplier = 0;
		avoiding = false;

		Vector3 sensorStartPos = transform.position;
		sensorStartPos += transform.forward * frontSensorPosition.z;
		sensorStartPos += transform.up * frontSensorPosition.y;

		//right front sensor
		sensorStartPos += transform.right * frontSideSensorPosition;
		if (Physics.Raycast (sensorStartPos, transform.forward, out hit, sensorLength)) {
			if(!hit.collider.CompareTag("TrafficLightBlock") && !hit.collider.CompareTag("Car")){
				Debug.DrawLine (sensorStartPos, hit.point);
				avoiding = true;
				avoidMultiplier -= 0.75f;
			}
		}


		//right angle sensor
		else if (Physics.Raycast (sensorStartPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength)) {
			if(!hit.collider.CompareTag("TrafficLightBlock")  && !hit.collider.CompareTag("Car")){
					Debug.DrawLine (sensorStartPos, hit.point);
					avoiding = true;
					avoidMultiplier -= 0.375f;
				}
		}

		//left front sensor
		sensorStartPos -= transform.right * frontSideSensorPosition * 2;
		if (Physics.Raycast (sensorStartPos, transform.forward, out hit, sensorLength)) {
			if(!hit.collider.CompareTag("TrafficLightBlock") && !hit.collider.CompareTag("Car")){
				Debug.DrawLine (sensorStartPos, hit.point);
				avoiding = true;
				avoidMultiplier += 0.75f;
			}

		}
		//left angle sensor
		else if (Physics.Raycast (sensorStartPos, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength)) {
			if(!hit.collider.CompareTag("TrafficLightBlock") && !hit.collider.CompareTag("Car")){
					Debug.DrawLine (sensorStartPos, hit.point);
					avoiding = true;
					avoidMultiplier += 0.375f;
				}

		}

		//front sensor
		sensorStartPos += transform.right * frontSideSensorPosition;
		if (avoidMultiplier == 0) {
			if (Physics.Raycast (sensorStartPos, transform.forward, out hit, sensorLength)) {
				if (!hit.collider.CompareTag("TrafficLightBlock") && !hit.collider.CompareTag("Car")) {
					Debug.DrawLine (sensorStartPos, hit.point);
					avoiding = true;
					if (hit.normal.x < 0) {
						avoidMultiplier = -1;
					} else {
						avoidMultiplier = 1;
					}
				}
			}
		}

		//reversing
		if (GetComponent<Rigidbody> ().velocity.magnitude < 2.0 && !isReversing && !isBraking && !trafficLightBrake && !carBrake) {
			reverseCounter += Time.deltaTime;
			if (reverseCounter >= waitToReverse) {
				reverseCounter = 0;
				isReversing = true;
			}
		} else {
			if (!isReversing) {
				reverseCounter = 0;
			} else {
				avoidMultiplier *= -1;
				reverseCounter += Time.deltaTime;
				if (reverseCounter >= reverseFor) {
					reverseCounter = 0;
					isReversing = false;
				}
			}
		}

		if (avoiding) {
			targetSteerAngle = maxSteerAngle * avoidMultiplier;
		}

	}


	private void ApplySteer(){

		if (avoiding)
			return;

		Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
		relativeVector /= relativeVector.magnitude;

		float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
		targetSteerAngle = newSteer;
	}

	private void LerpToSteerAngle(){
		wheelFL.steerAngle = Mathf.Lerp(wheelFL.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
		wheelFR.steerAngle = Mathf.Lerp(wheelFR.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
	}

	private void Drive(){
		
		currentSpeed = 2 * Mathf.PI * wheelFL.radius * wheelFL.rpm * 60 / 1000;
		carVelocity = transform.GetComponent<Rigidbody> ().velocity.magnitude;

		if (currentSpeed < maxSpeed && !isBraking) {
			if (!isReversing) {
				wheelFL.motorTorque = maxMotorTorque;
				wheelFR.motorTorque = maxMotorTorque;
			} else {
				wheelFL.motorTorque = -maxMotorTorque;
				wheelFR.motorTorque = -maxMotorTorque;
			}
		} else {
			wheelFL.motorTorque = 0;
			wheelFR.motorTorque = 0;
		}
	}

	private void CheckWaypointDistance(){

		if (Vector3.Distance (transform.position, nodes [currentNode].position) < minWaypointDistance) {

			if (currentNode == nodes.Count - 1) {
				currentNode = 0;
			} else {
				currentNode++;
			}
		} 
	}

	private float CheckWaypointAngle(Transform firstNode, Transform secondNode){
		
		float distance = Vector3.Distance (firstNode.position, secondNode.position);
		Vector3 temp = new Vector3(firstNode.position.x, secondNode.position.y, secondNode.position.z);
		float mapping = Vector3.Distance (firstNode.position, temp);
		float angle = Mathf.Acos (mapping / distance);
		angle = (angle * 180) / Mathf.PI;	
		if (angle > 45f)
			return 90 - angle;
		else
			return angle;
	}

	public void BeforeSteeringBrake(){
		if (currentNode < (nodes.Count - 1))
			steerAngle = CheckWaypointAngle (nodes[currentNode], nodes [currentNode + 1]);
		
		else {
			if (currentNode == (nodes.Count -1))
				steerAngle = CheckWaypointAngle (nodes [nodes.Count - 1], nodes [0]);
		}
		if (steerAngle>= 30f && currentSpeed >= 7f && Vector3.Distance (transform.position, nodes [currentNode].position) <= minSteerBrakeDistance) {
				isBraking = true;
		} else
				isBraking = false;
		}

	private void Braking(){
		if (isBraking || trafficLightBrake || carBrake) {
			carRenderer.material.mainTexture = textureBraking;
			wheelRL.brakeTorque = maxBrakeTorque;
			wheelRR.brakeTorque = maxBrakeTorque;
			if (currentSpeed <= 2)
				carRenderer.material.mainTexture = textureNormal;
		} else {
			carRenderer.material.mainTexture = textureNormal;
			wheelRL.brakeTorque = 0;
			wheelRR.brakeTorque = 0;
		}
	}
		
	private void Respawn(){
		float flipAngle = transform.localEulerAngles.z;
		if (GetComponent<Rigidbody> ().velocity.magnitude < 2 && !isBraking && !trafficLightBrake && !carBrake) {
			respawnCounter += Time.deltaTime;
			if (respawnCounter >= respawnWait) {
				if (currentNode == 0) {
					transform.position = nodes [nodes.Count - 1].position;
				} else {
					transform.position = nodes [currentNode - 1].position;
				}
				respawnCounter = 0;
				transform.Rotate(0, 0, -flipAngle);
			}
		}else respawnCounter = 0;
	}
} 
