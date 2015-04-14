﻿using UnityEngine;
using System.Collections;

public class DayNight : MonoBehaviour {

	public Color colorNight = Color.blue;
    public Color colorDay = Color.black;
    public float duration = 10F;

	IEnumerator NightTime () {
		yield return new WaitForSeconds(0);

		float lerp = Mathf.PingPong(Time.time, duration) / duration;
        RenderSettings.skybox.SetColor("_Tint", Color.Lerp(colorDay, colorNight, lerp));

        //RenderSettings.fog = true;
        //RenderSettings.fogColor = Color.black;
		
		Debug.Log("Night Time");
	}

	void Update() {
		StartCoroutine( NightTime() );
	}
}
