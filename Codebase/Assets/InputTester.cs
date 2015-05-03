using UnityEngine;
using System.Collections;

public class InputTester : MonoBehaviour {

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	void OnGUI() {
		GUI.Label( new Rect( 125, 125, 200, 20 ), string.Format( "Position: {0}", Input.mousePosition ) );
		GUI.Label( new Rect( 125, 145, 200, 20 ), string.Format( "Mouse button 0: {0}", Input.GetMouseButton( 0 ) ) );
		GUI.Label( new Rect( 125, 165, 200, 20 ), string.Format( "Mouse button 1: {0}", Input.GetMouseButton( 1 ) ) );
	}
}
