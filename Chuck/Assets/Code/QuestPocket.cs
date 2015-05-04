using UnityEngine;
using System.Collections;

public class QuestPocket : MonoBehaviour {

	private BaseQuest activeQuest;

	// Use this for initialization
	void Start () {
		MasterOfKnowledge.OnActivateQuest += MasterOfKnowledge_OnActivateQuest;
		MasterOfKnowledge.OnDeliverQuest += MasterOfKnowledge_OnDeliverQuest;
	}

	private void MasterOfKnowledge_OnDeliverQuest( object sender, QuestEventArgs e ) {
		
	}

	private void MasterOfKnowledge_OnActivateQuest( object sender, QuestEventArgs e ) {
		
	}


	// Update is called once per frame
	void Update () {
	
	}
}
