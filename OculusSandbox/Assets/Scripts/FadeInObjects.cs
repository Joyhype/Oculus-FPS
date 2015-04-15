using UnityEngine;
using System.Collections;

public class FadeInObjects : MonoBehaviour {

	void Start () {
		iTween.FadeTo(gameObject, iTween.Hash ("alpha", 0, "time", 8));
	}
}
