using UnityEngine;
using System.Collections;

public class Reticle : MonoBehaviour {

	public GameObject Gazer;
	public float HighlightDistance = 10;
	public float GazeDistance = 5;
	public float GazeTime = 1;

	private int gazeLayer;
	private bool ignoreGaze = false;
	private GameObject lastGazed = null;

	// Use this for initialization
	void Start() {
		gazeLayer = LayerMask.GetMask( "Interactable" );
	}

	// Update is called once per frame
	void Update() {
		RaycastHit info;
		if ( Physics.Raycast( transform.position, transform.forward, out info, HighlightDistance, gazeLayer ) ) {
			EnableGazeReticle();

			var gazed = info.collider.gameObject;
			if ( gazed != lastGazed ) {
				ignoreGaze = false;
				ResetScale();

				if ( lastGazed != null ) {
					lastGazed.BroadcastMessage( "OnLostGaze", SendMessageOptions.DontRequireReceiver );
				}

				lastGazed = gazed;
				lastGazed.BroadcastMessage( "OnGaze", SendMessageOptions.DontRequireReceiver );
			} else {
				if ( info.distance > GazeDistance ) {
					ignoreGaze = false;
					ResetScale();
					return;
				}

				if ( ignoreGaze ) {
					ResetScale();
					return;
				}

				var scale = Mathf.Min( Gazer.transform.localScale.x + ( Time.deltaTime * ( 1 / GazeTime ) ), 1 );
				Gazer.transform.localScale = new Vector3( scale, scale, 1 );

				if ( scale == 1 ) {
					ignoreGaze = true;
					lastGazed.BroadcastMessage( "OnGazed", SendMessageOptions.RequireReceiver );
				}
			}
		} else {
			ignoreGaze = false;

			if ( lastGazed != null ) {
				lastGazed.BroadcastMessage( "OnLostGaze", SendMessageOptions.DontRequireReceiver );
				lastGazed = null;
			}

			if ( ResetScale() ) {
				DisableGazeReticle();
			}
		}
	}

	private void EnableGazeReticle() {
		Gazer.SetActive( true );
	}

	private void DisableGazeReticle() {
		Gazer.transform.localScale = new Vector3( 0.2f, 0.2f, 1 );
		Gazer.SetActive( false );
	}

	private bool ResetScale() {
		var scale = Mathf.Max( Gazer.transform.localScale.x - ( Time.deltaTime * 5f ), 0.2f );
		Gazer.transform.localScale = new Vector3( scale, scale, 1 );

		return scale == 0.2f;
	}
}