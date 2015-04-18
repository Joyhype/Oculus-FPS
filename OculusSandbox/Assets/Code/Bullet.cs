using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public float speed = 10f;

	// Update is called once per frame
	void Update() {
		transform.position += transform.forward * speed * Time.deltaTime;
	}

	void OnTriggerEnter( Collider other ) {
		if ( LayerMask.LayerToName( other.gameObject.layer ) == "Killable" ) {
			InformationHandler.KilledMob( other.gameObject.tag );
		}
	}
}
