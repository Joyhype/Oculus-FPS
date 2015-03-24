using UnityEngine;
using System.Collections;

public class RemoveBullets : MonoBehaviour {

	public GameObject bullet; 

	IEnumerator Start() {
		yield return new WaitForSeconds(5);
		Destroy(bullet);
	}
}