using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

public class DryThroat : MonoBehaviour {

	public UnityEvent OnThirstTick;
	public UnityEvent OnThirstDepleted;

	public static event EventHandler<GameObjectEventArgs> OnThirstTickEvent;
	public static event EventHandler<GameObjectEventArgs> OnThirstDepletedEvent;

	public float TickDuration = 1;
	public int Amount = 1;

	private float timer;

	// Use this for initialization
	void Start() {
		timer = TickDuration;
	}

	// Update is called once per frame
	void Update() {
		if ( Amount <= 0 ) {
			return;
		}

		if ( timer <= 0 ) {
			Amount -= 1;

			OnThirstTick.Invoke();

			if ( OnThirstTickEvent != null ) {
				OnThirstTickEvent.Invoke( this, new GameObjectEventArgs( gameObject ) );
			}

			if ( Amount == 0 ) {
				OnThirstDepleted.Invoke();

				if ( OnThirstDepletedEvent != null ) {
					OnThirstDepletedEvent.Invoke( this, new GameObjectEventArgs( gameObject ) );
				}
			}
		} else {
			timer -= Time.deltaTime;
		}
	}
}