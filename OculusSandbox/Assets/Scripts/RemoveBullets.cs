using UnityEngine;
using System.Collections;

public class RemoveBullets : MonoBehaviour {


	IEnumerator Start() {
		yield return new WaitForSeconds(5);
		Destroy(gameObject);
	}
}