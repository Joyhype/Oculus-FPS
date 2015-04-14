using UnityEngine;
using System.Collections;

public class BisonDeath : MonoBehaviour {

	public GameObject bullet; 
	
	void Start () {
	
	}

	void onCollisionEnter (Collision col) {
		if (col.gameObject.name == "gun") {
			Debug.Log("Bullet Hit");
		}
	}
}
