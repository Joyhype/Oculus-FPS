using System;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class LunarCycle : MonoBehaviour {

	public float Angle = 0;
	public int Duration = 60;
	public float Radius = 250;

	[Range(90, 270)]
	public float SetHorizonAngle = 190;
	[Range(270, 450)]
	public float RiseHorizonAngle = 360;

	public UnityEvent OnDayPass;
	public UnityEvent OnRise;
	public UnityEvent OnSet;

	public static event EventHandler<GameObjectEventArgs> OnDayPassEvent;
	public static event EventHandler<GameObjectEventArgs> OnRiseEvent;
	public static event EventHandler<GameObjectEventArgs> OnSetEvent;

	private Vector3 centerPoint;

	private float step = 0;

	private float startingAngle = 0;

	// Use this for initialization
	void Start() {
		centerPoint = new Vector3();
		step = 60f / Duration;
		startingAngle = Angle;
	}

	void Update() {
		transform.LookAt( centerPoint );

		if ( Application.isEditor && !Application.isPlaying ) {
			CalculateAngleAndPosition();
		}
	}

	void FixedUpdate() {
		var prevAngle = Angle;
		Angle += step * Time.deltaTime;

		if ( prevAngle < SetHorizonAngle && Angle >= SetHorizonAngle ) {
			OnSet.Invoke();

			if ( OnSetEvent != null ) {
				OnSetEvent.Invoke( this, new GameObjectEventArgs( gameObject ) );
			}
		} else if ( prevAngle < RiseHorizonAngle && Angle >= RiseHorizonAngle ) {
			OnRise.Invoke();

			if ( OnRiseEvent != null ) {
				OnRiseEvent.Invoke( this, new GameObjectEventArgs( gameObject ) );
			}
		}

		if ( prevAngle < startingAngle && Angle >= startingAngle ) {
			OnDayPass.Invoke();

			if ( OnDayPassEvent != null ) {
				OnDayPassEvent.Invoke( this, new GameObjectEventArgs( gameObject ) );
			}
		}

		Angle = Angle % RiseHorizonAngle;

		CalculateAngleAndPosition();
	}

	private void CalculateAngleAndPosition() {
		var r = Mathf.Deg2Rad * Angle;
		var x = Mathf.Cos( r ) * Radius;
		var y = Mathf.Sin( r ) * Radius;

		transform.position = new Vector3( x, y );
	}
}
