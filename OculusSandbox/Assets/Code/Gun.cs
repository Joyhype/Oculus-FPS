using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {

	public GameObject BulletPrefab;
	public GameObject MuzzleFlashPrefab;

	public GameObject GunTip;
	public int Ammo = 3;

	public float DelayBetweenShot = 0.5f;
	private float timer = 0;

	public float GunSway = 0.02f;

	private float originalZ;

	// Use this for initialization
	void Start() {
		originalZ = transform.localPosition.z;

		iTween.MoveBy( gameObject,
			iTween.Hash( "y", -GunSway, "time", 2f,
			"looptype", iTween.LoopType.pingPong,
			"easetype", iTween.EaseType.easeInOutBack ) );
	}

	// Update is called once per frame
	void Update() {
		if ( timer > 0 ) {
			timer -= Time.deltaTime;
		}

		if ( Input.GetMouseButtonDown( 0 ) ) {
			if ( Ammo > 0 && timer <= 0 ) {

				Instantiate( BulletPrefab, GunTip.transform.position, transform.rotation );
				Instantiate( MuzzleFlashPrefab, GunTip.transform.position, transform.rotation );

				timer = DelayBetweenShot;
				Ammo--;

				iTween.MoveBy( gameObject,
					iTween.Hash("name", "recoilOne", "z", -0.15f, "time", 0.1f,
					"easetype", iTween.EaseType.easeOutElastic ) );
				
				iTween.MoveBy( gameObject,
					iTween.Hash("name", "recoilTwo", "z", 0.15f, "time", 0.1f, "delay", 0.11f,
                    "easetype", iTween.EaseType.linear ) );
			}
		}
	}
}
