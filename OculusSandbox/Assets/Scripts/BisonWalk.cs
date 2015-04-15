using UnityEngine;
using System.Collections;

public class BisonWalk : MonoBehaviour {

	void Start() {	
		/*iTween.MoveBy (this.gameObject, iTween.Hash ("z", 250.0f, 
			                                 "time", 4f, 
			                                 "looptype", iTween.LoopType.pingPong,
			                                 "easetype", iTween.EaseType.easeInOutSine
		));*/

		iTween.MoveBy (this.gameObject, iTween.Hash (
											"z", 450f,
											"time", 4f, 
											"looptype", iTween.LoopType.none,
											"easetype", iTween.EaseType.easeInOutSine,
											"oncomplete", "rotateBison",
											"oncompletetarget", this.gameObject
		));
	}

	void rotateBison() {
		/*iTween.RotateBy (this.gameObject, iTween.Hash (
											"y", 90f, 
											"time", 20f, 
											"looptype", iTween.LoopType.none,
											"easetype", iTween.EaseType.easeInOutSine
       	));*/
	}


}
