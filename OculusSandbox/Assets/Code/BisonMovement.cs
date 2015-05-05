using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BisonMovement : MonoBehaviour {

	public float Radius;
	public float RotationDelay = 0.5f;
	public MinMaxRange RotationWait;
	public float MoveSpeed = 5f;
	[Range(0, 1)]
	public float ChanceOfWalking = 0.5f;
	public MinMaxRange StayDuration;

	private Vector3 originalPosition;

	// Use this for initialization
	void Start() {
		if ( Application.isPlaying ) {
			originalPosition = transform.position;
			RollDice();
		}
	}

	void Update() {
		if ( !Application.isPlaying ) {
			DrawCircle();
		}
	}

	private void DrawCircle() {
		var center = transform.position;
		var steps = 20;
		var angleStep = 360 / steps;
		var color = Color.green;

		for ( int i = 0; i < steps; i++ ) {
			var startAngle = i * angleStep * Mathf.Deg2Rad;
			var endAngle = ( i + 1 ) * angleStep * Mathf.Deg2Rad;

			var startPos = new Vector3( Mathf.Cos( startAngle ), 0, Mathf.Sin( startAngle ) ) * Radius;
			var endPos = new Vector3( Mathf.Cos( endAngle ), 0, Mathf.Sin( endAngle ) ) * Radius;

			Debug.DrawLine( startPos + center, endPos + center, color );
		}
	}

	private void RollDice() {
		var outcome = Random.Range( 0, 100 ) / 100f;

		if ( outcome <= ChanceOfWalking ) {
			MoveToPosition();
		} else {
			StartCoroutine( WaitForSeconds() );
		}
	}

	private void MoveToPosition() {
		var position = GetNewPosition();

		iTween.LookTo( gameObject, iTween.Hash( "time", RotationDelay, "looktarget", position, "easetype", iTween.EaseType.easeInOutSine ) );
		iTween.MoveTo( gameObject, iTween.Hash( "delay", RotationDelay + RotationWait.GetValue(), "speed", MoveSpeed, "position", position, "oncomplete", "RollDice", "easetype", iTween.EaseType.easeInOutSine ) );
	}

	private Vector3 GetNewPosition() {
		var p = Random.insideUnitCircle * Radius;
		return new Vector3( p.x, originalPosition.y, p.y );
	}

	private IEnumerator WaitForSeconds() {
		yield return new WaitForSeconds( StayDuration.GetValue() );
		RollDice();
	}
}
