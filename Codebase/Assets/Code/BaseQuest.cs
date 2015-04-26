using UnityEngine;
using System.Collections;

public class BaseQuest : MonoBehaviour {

	public bool IsActive = false;
	public bool IsDone = false;

	void Awake() {
		MasterOfKnowledge.OnItemPicked += OnItemPicked;
		MasterOfKnowledge.OnMobKilled += OnMobKilled;
	}

	protected virtual void OnItemPicked( object sender, ItemEventArgs e ) { }
	protected virtual void OnMobKilled( object sender, MobEventArgs e ) { }

	public virtual void Activate() {
		MasterOfKnowledge.ActivateQuest( this );

		IsActive = true;
	}

	public void FinishQuest() {
		IsDone = true;
	}
}
