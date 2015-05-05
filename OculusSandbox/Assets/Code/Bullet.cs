using UnityEngine;

[ExecuteInEditMode]
public class Bullet : MonoBehaviour {

	public float Speed = 10f;

	// Update is called once per frame
	void Update() {
		if ( Application.isEditor ) {
			Debug.DrawLine( transform.position, transform.position - transform.forward * 5, Color.black );
		}

		if ( Application.isPlaying ) {
			transform.position -= transform.forward * Speed * Time.deltaTime;
		}
	}

	void OnTriggerEnter( Collider other ) {
		if ( LayerMask.LayerToName( other.gameObject.layer ) == "Killable" ) {
			MasterOfKnowledge.KilledMob( other.gameObject.tag );
		}

		other.gameObject.BroadcastMessage( "OnKill" );
	}
}
