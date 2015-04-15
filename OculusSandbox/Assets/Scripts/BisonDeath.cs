using UnityEngine;
using System.Collections;

public class BisonDeath : MonoBehaviour {

	public GameObject bullet; 
	public GameObject bison;
	
	void Start () {}

	void OnCollisionEnter(Collision c) {
		if (c.gameObject.tag == "Bullet") {
			Debug.Log("Bullet Hit");
		}
	}
}
