using UnityEngine;
using System.Collections;

public class BisonWalk : MonoBehaviour {

	void Start() {	
		iTween.MoveBy (this.gameObject, iTween.Hash (
											"z", 450f,
											"time", 4f, 
											"looptype", iTween.LoopType.none,
											"easetype", iTween.EaseType.easeInOutSine,
											"oncomplete", "RotateBison",
											"oncompletetarget", this.gameObject
		));
	}

	void RotateBison() {
		iTween.RotateTo (this.gameObject, iTween.Hash (
											"y", -360f, 
											"time", 1f, 
											"looptype", iTween.LoopType.none,
											"easetype", iTween.EaseType.easeInOutSine,
											"oncomplete", "WalkBack",
											"oncompletetarget", this.gameObject
       	));
	}

	void RotateBison2() {
		iTween.RotateTo (this.gameObject, iTween.Hash (
											"y", 180f, 
											"time", 1f, 
											"looptype", iTween.LoopType.none,
											"easetype", iTween.EaseType.easeInOutSine,
											"oncomplete", "WalkBack2",
											"oncompletetarget", this.gameObject
       	));
	}

	void WalkBack() {
			iTween.MoveBy (this.gameObject, iTween.Hash (
											"z", 450f,
											"time", 4f, 
											"looptype", iTween.LoopType.none,
											"easetype", iTween.EaseType.easeInOutSine,
											"oncomplete", "RotateBison2",
											"oncompletetarget", this.gameObject
		));
	}

	void WalkBack2() {
			iTween.MoveBy (this.gameObject, iTween.Hash (
											"z", 450f,
											"time", 4f, 
											"looptype", iTween.LoopType.none,
											"easetype", iTween.EaseType.easeInOutSine,
											"oncomplete", "RotateBison",
											"oncompletetarget", this.gameObject
		));
	}
}
