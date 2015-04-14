using UnityEngine;
using System.Collections;

public class BulletHandler : MonoBehaviour {

	public float speed = 50f;
	public Rigidbody projecticle;
	public GameObject gun;

	
	public float delayedTime = 2f;
	private float counter = 2f;

	private int ammo = 10000;
	private float pullback = 5f;
	
	Vector3 startPos;

	public GameObject muzzle;

	void Awake() {
		muzzle = GameObject.Find("MuzzleFlash");
		muzzle.SetActive(false);
	}

	void Start() {
		startPos = gun.transform.position;
	}

	void Update () {
		fireBullet();
		muzzle.transform.Rotate(Vector3.up * Time.deltaTime, Space.World);

		//Reset Gun After Recoil;

 	}

	void fireBullet() {
		if (Input.GetMouseButtonDown(0) && counter > delayedTime) {
			if ( ammo >= 1 ) {
				
				counter = 0;
				ammo --;
				
				Rigidbody instantiatedProjectile = Instantiate(projecticle, transform.position, transform.rotation) as Rigidbody;
				instantiatedProjectile.AddForce(transform.forward * speed);
				
				muzzle.SetActive(true);
				muzzle.GetComponent<ParticleSystem>().Play();
	
				GetComponent<AudioSource>().Play();

				iTween.MoveBy (gun, iTween.Hash ("z", 8.0f, 
				                                 "time", 0.1f, 
				                                 "looptype", iTween.LoopType.none,
				                                 "easetype", iTween.EaseType.easeInElastic,
				                                 "oncomplete", "recoilOver",
				                                  "oncompletetarget", this.gameObject));

			} else if ( ammo == 0) {
				Debug.Log("NO AMMO");
			}
		} 
		
		counter += 10 * Time.deltaTime;
		
		if( muzzle.activeInHierarchy ) {
			StartCoroutine( StartRemoveMuzzle() );
		}
	
	}

	void recoilOver() {
		iTween.MoveBy (gun, iTween.Hash ("z", -8.0f, 
		                                 "time", 0.3f, 
		                                 "looptype", iTween.LoopType.none,
		                                 "easetype", iTween.EaseType.easeInOutSine));

	}

	 IEnumerator StartRemoveMuzzle() {
         yield return new WaitForSeconds(0.2f);
         Debug.Log("Kill Muzzle Flash");
         
         muzzle.GetComponent<ParticleSystem>().Stop();
         muzzle.SetActive(false);

         //Reset Gun After Recoil
		 //gun.transform.position += new Vector3 (0, 0, -pullback );
     }
}
