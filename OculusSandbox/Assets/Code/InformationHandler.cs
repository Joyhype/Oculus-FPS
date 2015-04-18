using UnityEngine;
using System.Collections;
using System;

public class InformationHandler : MonoBehaviour {

	public static event EventHandler<MobEventArgs> OnMobKilled;
	public static event EventHandler<ItemEventArgs> OnItemPicked;

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
}
