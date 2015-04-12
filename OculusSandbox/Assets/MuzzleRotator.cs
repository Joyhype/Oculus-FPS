using UnityEngine;
using System.Collections;

public class MuzzleRotator : MonoBehaviour {

	public float RotationSpeed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//transform.RotateAround (new Vector3 (0, 0, 1), RotationSpeed * Time.deltaTime);
		transform.localRotation = Quaternion.Euler (0, 0, (RotationSpeed * Time.deltaTime) + transform.localRotation.eulerAngles.z);
	}
}
