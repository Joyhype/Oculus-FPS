using UnityEngine;
using System.Collections;

public class Quests : MonoBehaviour {

	public bool playerDoingQuest = false;
	public bool questCompleted = false; 
	
	public _Quests getQuest;
    
    public enum _Quests {
        None,
        Cactus,
        Bison
    }
  
    void CheckForQuest() {
        if (getQuest == _Quests.Bison) {
            playerDoingQuest = true; 
        }
        
        if (getQuest == _Quests.Cactus) {
        	playerDoingQuest = true; 
        }

        else if (getQuest == _Quests.None) {
        	playerDoingQuest = false; 
        }
    }

	void InQuest() {
		if (playerDoingQuest == true) {
			//Debug.Log("Player is Doing a quest");
		} else if (playerDoingQuest == false) {
			//Debug.Log("Player is Not Doing A Quest");
		}
	} 

	void Update() {
		InQuest(); 
		CheckForQuest();
		isQuestComplete();
	}

	void isQuestComplete() {
		if (questCompleted == true) {
			Debug.Log("Player has completed a quest");

			//Do save data 
			//Mark Quest Complete
		}
	}
}