using UnityEngine;
using System.Collections;

public class Pippa : MonoBehaviour {

	public GameObject BulletPrefab;
	public GameObject MuzzleFlashPrefab;

	public GameObject GunTip;

	public float ShotDelay = 0.5f;
	public float ReloadDelay = 2.5f;

	public float GunSwayDistance = 0.02f;
	public float RecoilDistance = 0.15f;

	private float reloadTimer = 0;
	private float shotTimer = 0;
	private int currentAmmo = 3;

	// Use this for initialization
	void Start() {
		//if ( BulletPrefab == null || MuzzleFlashPrefab == null || GunTip == null ) {
		//	Debug.LogError( "Please put in the prefabs and the gun tip" );
		//	Destroy( gameObject );
		//}

		iTween.MoveBy( gameObject,
			iTween.Hash( "y", -GunSwayDistance, "time", 2f,
			"looptype", iTween.LoopType.pingPong,
			"easetype", iTween.EaseType.easeInOutBack ) );
	}

	// Update is called once per frame
	void Update() {
		if ( reloadTimer > 0 ) {
			reloadTimer -= Time.deltaTime;
		} else {
			if ( shotTimer > 0 ) {
				shotTimer -= Time.deltaTime;
			} else {
				if ( Input.GetMouseButtonDown( 0 ) ) {
					Instantiate( BulletPrefab, GunTip.transform.position, transform.rotation );
					//Instantiate( MuzzleFlashPrefab, GunTip.transform.position, transform.rotation );

					currentAmmo--;

					if ( currentAmmo == 0 ) {
						reloadTimer = ReloadDelay;
						currentAmmo = 3;
					} else {
						shotTimer = ShotDelay;
					}

					iTween.MoveBy( gameObject,
						iTween.Hash( "name", "recoilOne", "z", -0.15f, "time", 0.1f,
						"easetype", iTween.EaseType.easeOutElastic ) );
					iTween.MoveBy( gameObject,
						iTween.Hash( "name", "recoilTwo", "z", 0.15f, "time", 0.1f, "delay", 0.11f,
						"easetype", iTween.EaseType.linear ) );
				}
			}
		}
	}
}
