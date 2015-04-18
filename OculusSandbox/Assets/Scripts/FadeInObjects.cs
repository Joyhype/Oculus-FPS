using UnityEngine;
using System.Collections;

public class FadeInObjects : MonoBehaviour {

	public float fadeOutTime = 5f;

	void Start () {
		iTween.FadeTo(gameObject, iTween.Hash ("alpha", 0, "time", fadeOutTime));
	}
}
