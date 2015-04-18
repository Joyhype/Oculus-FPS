using UnityEngine;
using System.Collections;

public class BaseQuest : MonoBehaviour {

	public bool IsActive = false;
	public bool IsDone = false;

	void Awake() {
		InformationHandler.OnItemPicked += OnItemPicked;
		InformationHandler.OnMobKilled += OnMobKilled;
	}

	public virtual void Activate() {
		IsActive = true;
	}

	public virtual void OnItemPicked( object sender, InformationHandler.ItemEventArgs args ) { }
	public virtual void OnMobKilled( object sender, InformationHandler.MobEventArgs args ) { }
}
