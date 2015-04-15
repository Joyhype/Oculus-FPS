using UnityEngine;
using System.Collections;

public class BisonDeath : MonoBehaviour {

	public GameObject bison;

	void Awake() {
		bison = GameObject.Find("bison");
	}
	void OnCollisionEnter (Collision c) {

		if (c.gameObject.tag == "Bullet") {
			Debug.Log("Bullet Hit");
			SpawnBlood();
			
			iTween.RotateTo (this.gameObject, iTween.Hash (
											"z", 90f, 
											"time", 0.4f,
											"y", -10f,
											"x", 50f,
											"looptype", iTween.LoopType.none,
											"easetype", iTween.EaseType.easeInOutSine
       		));

       		iTween.MoveBy (this.gameObject, iTween.Hash (
											"y", -65f,
											"time", 0.4f, 
											"looptype", iTween.LoopType.none,
											"easetype", iTween.EaseType.easeInOutSine
			));
		}
	}

	void SpawnBlood () {
		//bison.GetComponent<ParticleSystem>().Play();
	}
}
