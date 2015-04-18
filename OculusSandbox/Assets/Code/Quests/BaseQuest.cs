using UnityEngine;
using System.Collections;

public class BaseQuest : MonoBehaviour {

	public bool IsActive;
	public bool IsDone;

	void Awake() {
		InformationHandler.OnItemPicked += OnItemPicked;
		InformationHandler.OnMobKilled += OnMobKilled;
	}

	public virtual void OnItemPicked( object sender, InformationHandler.ItemEventArgs args ) { }

	public virtual void OnMobKilled( object sender, InformationHandler.MobEventArgs args ) { }
}
