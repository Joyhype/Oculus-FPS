using UnityEngine;
using System.Collections;

public class GunSway : MonoBehaviour {

	public GameObject gun;
	public float sway = 0.02f;

	// Update is called once per frame
	void FixedUpdate () {
		
//		gun.transform.position += new Vector3 (0, sway, 0 );
//
//		if (gun.transform.position.y > 9f) {
//			sway = -0.02f;
//		} else if (gun.transform.position.y < 7f) {
//			sway = 0.02f;
//		}
	}

	void Start() {
		iTween.MoveBy (gun, iTween.Hash ("y", -0.1f, 
		                                      "time", 3f, 
		                                      "looptype", iTween.LoopType.pingPong,
		                                      "easetype", iTween.EaseType.easeInOutBack));
	}
}
