using UnityEngine;
using System.Collections;

public class QuestGiver : MonoBehaviour {

	public GameObject[] AvailableQuests;

	private GameObject player;

	// Use this for initialization
	void Start() {

	}

	void OnGazed() {
		// Show cards
	}

	private bool isGaze = false;

	void OnGaze() {
		isGaze = true;
	}

	void OnLostGaze() {
		isGaze = false;
	}
}
