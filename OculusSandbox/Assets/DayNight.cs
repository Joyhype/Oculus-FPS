using UnityEngine;
using System.Collections;

public class DayNight : MonoBehaviour {

	public Color colorStart = Color.blue;
    public Color colorEnd = Color.black;
    public float duration = 20F;

	IEnumerator NightTime () {
		yield return new WaitForSeconds(0);

		float lerp = Mathf.PingPong(Time.time, duration) / duration;
        RenderSettings.skybox.SetColor("_Tint", Color.Lerp(colorEnd, colorStart, lerp));
		
		Debug.Log("Night Time");
	}

	void Update() {
		StartCoroutine( NightTime() );
	}
}
