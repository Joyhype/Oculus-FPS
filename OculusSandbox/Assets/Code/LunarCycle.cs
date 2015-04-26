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

		if ( Application.isEditor && !Application.isPlaying ) {
			CalculateAngleAndPosition();
		}
	}

	void FixedUpdate() {
		var prevAngle = Angle;
		Angle += step * Time.deltaTime;

		if ( prevAngle < 195 && Angle >= 195 ) {
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
