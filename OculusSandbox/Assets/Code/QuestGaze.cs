using UnityEngine;
using System.Collections;

public class QuestGaze : MonoBehaviour {

	private Vector3 originalScale;

	// Use this for initialization
	void Start() {
		originalScale = transform.localScale;
	}

	void OnGaze() {
		if ( ignore ) return;

		iTween.StopByName( gameObject, "down" );
		iTween.ScaleTo( gameObject, iTween.Hash( "name", "up", "scale", originalScale * 1.5f, "time", 0.5f ) );
	}

	void OnLostGaze() {
		if ( ignore ) return;

		iTween.StopByName( gameObject, "up" );
		iTween.ScaleTo( gameObject, iTween.Hash( "name", "down", "scale", originalScale, "time", 0.5f ) );
	}

	private bool isVisible = false;
	private bool ignore = false;

	void OnGazed() {
		if ( ignore ) return;

		if ( isVisible ) {
			InformationHandler.SetActiveQuest( GetComponent<BaseQuest>() );

			iTween.ScaleTo( gameObject, new Vector3(), 1f );
			iTween.RotateBy( gameObject, iTween.Hash( "amount", new Vector3( 0, 2.5f ), "time", 1f, "easetype", iTween.EaseType.linear ) );
			StartCoroutine( DestroyGameObject() );
			ignore = true;
		} else {
			iTween.RotateBy( gameObject, new Vector3( 0, 0.5f, 0 ), 1f );
			isVisible = !isVisible;
		}
	}

	private IEnumerator DestroyGameObject() {
		yield return new WaitForSeconds( 1 );
		Destroy( gameObject );
	}
}
