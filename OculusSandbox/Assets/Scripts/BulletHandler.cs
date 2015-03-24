using UnityEngine;
using System.Collections;

public class BulletHandler : MonoBehaviour {

	public float speed = 50f;
	public Rigidbody projecticle; 
	
	public float delayedTime = 0.1f;
	private float counter = 0;

	void Update () {
		fireBullet();
	}

	void fireBullet() {
		if (Input.GetMouseButtonDown(0) && counter > delayedTime) {

			Rigidbody instantiatedProjectile = Instantiate(projecticle, transform.position, transform.rotation) as Rigidbody;
			instantiatedProjectile.AddForce(transform.forward * speed);

			counter = 0;

			GetComponent<AudioSource>().Play();
		}

		//Spacing Between Bullet Fire
		counter += 10 * Time.deltaTime;
	}
}
