using UnityEngine;
using System.Collections;

public class BisonDeath : MonoBehaviour {

	public GameObject bullet; 
	
	void Start () {
	
	}

	void onCollisionEnter (Collision col) {
		if (col.gameObject.name == "bullet") {
			Debug.Log("Bullet Hit");
		}
	}
}
