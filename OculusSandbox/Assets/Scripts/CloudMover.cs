using UnityEngine;
using System.Collections;

public class CloudMover : MonoBehaviour {

	public GameObject c1;

	public float speed = 2;

	void Update () {
		c1.transform.position += new Vector3( -speed, 0, 0);	
	}
}
