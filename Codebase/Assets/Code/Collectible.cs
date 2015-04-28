using UnityEngine;
using System.Collections;

public class Collectible : MonoBehaviour {

	void OnPickup() {
		iTween.RotateBy( gameObject, iTween.Hash( "amount", new Vector3( 0, 3, 0 ), "time", 1f, "easetype", iTween.EaseType.easeOutSine ) );
		iTween.ScaleTo( gameObject, iTween.Hash( "scale", new Vector3( 0, 0, 0 ), "time", 1f, "easetype", iTween.EaseType.linear ) );
		StartCoroutine( DestroyMe( 1 ) );
	}

	private IEnumerator DestroyMe( float time ) {
		yield return new WaitForSeconds( time );
		Destroy( gameObject );
	}
}
