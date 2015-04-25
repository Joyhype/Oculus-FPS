using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[ExecuteInEditMode]
public class LunarCycle : MonoBehaviour {

	public float Angle = 0;
	public int Duration = 60;
	public float Radius = 250;

	public UnityEvent OnRise;
	public UnityEvent OnSet;

	private Vector3 centerPoint;

	private float step = 0;
	private float timer = 0;

	// Use this for initialization
	void Start() {
		centerPoint = new Vector3();

		step = 60f / Duration;
	}

	// Update is called once per frame
	void Update() {
		transform.LookAt( centerPoint );
		//Don't actually need to lerp
		//transform.position = Vector3.Lerp( transform.position, newPosition, 0.1f );

		if ( Application.isEditor && !Application.isPlaying ) {
			CalculateAngleAndPosition();
		}
	}

	void FixedUpdate() {
		var prevAngle = Angle;
		Angle += step * Time.deltaTime;

		if ( prevAngle < 190 && Angle >= 190 ) {
			OnSet.Invoke();
		} else if ( prevAngle < 360 && Angle >= 360 ) {
			OnRise.Invoke();
		}

		Angle = Angle % 360;

		CalculateAngleAndPosition();
	}

	private void CalculateAngleAndPosition() {
		var r = Mathf.Deg2Rad * Angle;
		var x = Mathf.Cos( r ) * Radius;
		var y = Mathf.Sin( r ) * Radius;

		transform.position = new Vector3( x, y );
	}
}
