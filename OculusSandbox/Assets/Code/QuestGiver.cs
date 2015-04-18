using UnityEngine;
using System.Collections;

public class QuestGiver : MonoBehaviour {

	public GameObject[] AvailableQuests;

	private GameObject player;
	private bool showingQuests;

	// Use this for initialization
	void Start() {
		player = GameObject.FindGameObjectWithTag( "Player" );
	}

	// Update is called once per frame
	void Update() {
		//showingQuests = ( transform.position - player.transform.position ).sqrMagnitude < 5;
		
	}

	void OnGazed() {
		Debug.Log( "HIT" );
	}

	void OnGaze() {
		Debug.Log( "Gaze" );
	}

	void OnLostGaze() {
		Debug.Log( "Lost" );
	}
}
