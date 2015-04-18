using UnityEngine;
using System.Collections;

public class RotateObject : MonoBehaviour {

	public float rotateSpeed = 50f;
	
	void Start () {
		iTween.RotateBy (this.gameObject, iTween.Hash (
											"y", rotateSpeed, 
											"time", 1f,
											"looptype", iTween.LoopType.pingPong,
											"easetype", iTween.EaseType.easeInOutSine
       	));
	}
}
