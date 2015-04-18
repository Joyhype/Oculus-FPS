using UnityEngine;
using System.Collections;

public class MuzzleFlash : MonoBehaviour {

	public float TimeToDestroy = 0.2f;
	private float timer = 0;

	// Update is called once per frame
	void Update() {
		timer += Time.deltaTime;

		if ( timer >= TimeToDestroy ) {
			Destroy( gameObject );
		}
	}
}
