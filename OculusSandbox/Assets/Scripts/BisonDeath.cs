using UnityEngine;
using System.Collections;

public class BisonDeath : MonoBehaviour {

	private bool alive;

	void Start() {
		alive = true;
	}

	void OnCollisionEnter (Collision c) {

		//Spawn blood and move bison 
		if (c.gameObject.tag == "Bullet") {

			alive = false; 

			if (alive == false) {
				
				Debug.Log("Bullet Hit");
				SpawnBlood();
				
				//Rotate
				iTween.RotateTo (this.gameObject, iTween.Hash (
												"z", 90f, 
												"time", 0.4f,
												"y", -10f,
												"looptype", iTween.LoopType.none,
												"easetype", iTween.EaseType.easeInOutSine
	       		));

				//Stuff Bison onto the Ground
	       		iTween.MoveBy (this.gameObject, iTween.Hash (
												"y", -65f,
												"time", 0.4f, 
												"looptype", iTween.LoopType.none,
												"easetype", iTween.EaseType.easeInOutSine
				));
       		} else {
       			SpawnBlood();
       		}
		}
	}

	void SpawnBlood () {
		gameObject.GetComponentInChildren<ParticleSystem>().Play();
	}
}
