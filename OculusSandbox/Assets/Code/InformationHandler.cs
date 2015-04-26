using UnityEngine;
using System.Collections;
using System;

public class InformationHandler : MonoBehaviour {

	public static event EventHandler<MobEventArgs> OnMobKilled;
	public static event EventHandler<ItemEventArgs> OnItemPicked;
	public static event EventHandler<QuestEventArgs> OnActivateQuest;

	public class MobEventArgs : EventArgs {

		public readonly string Tag;

		public MobEventArgs( string tag ) : base() {
			Tag = tag;
		}
	}
	
	public class ItemEventArgs : EventArgs {

		public readonly string Tag;

		public ItemEventArgs( string tag ) : base() {
			Tag = tag;
		}
	}
	public class QuestEventArgs : EventArgs {
		public readonly BaseQuest Quest;

		public QuestEventArgs( BaseQuest quest ) : base() {
			Quest = quest;
		}
	}

	public static void KilledMob( string tag ) {
		if ( OnMobKilled != null ) {
			OnMobKilled.Invoke( null, new MobEventArgs( tag ) );
		}
	}

	public static void ItemPicked( string tag ) {
		if ( OnItemPicked != null ) {
			OnItemPicked.Invoke( null, new ItemEventArgs( tag ) );
		}
	}

	public static void SetActiveQuest( BaseQuest quest ) {
		if ( OnActivateQuest != null ) {
			OnActivateQuest.Invoke( null, new QuestEventArgs( quest ) );
		}
	}
}
