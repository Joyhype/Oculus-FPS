using UnityEngine;
using System.Collections;

public class Blinker : MonoBehaviour {

	public float Delay = 0;
	public float InitialBlinkTime = 0.5f;

	public bool IncreaseBlinkSpeed = false;

	private float blinkTime = 0;
	private float timer = 0;
	private bool doBlink = false;

	new private Renderer renderer;

	// Use this for initialization
	void Start() {
		StartCoroutine( StartWithDelay() );

		renderer = GetComponent<Renderer>();

		blinkTime = InitialBlinkTime;
	}

	// Update is called once per frame
	void Update() {
		if ( doBlink ) {
			if ( timer > 0 ) {
				timer -= Time.deltaTime;
			} else {
				renderer.enabled = !renderer.enabled;

				if ( renderer.enabled && IncreaseBlinkSpeed ) {
					blinkTime *= 0.9f;

					if ( blinkTime < 0.01f ) {
						Destroy( gameObject );
					}
				}

				timer = blinkTime;
			}
		}
	}

	private IEnumerator StartWithDelay() {
		yield return new WaitForSeconds( Delay );

		doBlink = true;

		yield break;
	}
}
