using System;

public class MasterOfKnowledge {

	public static event EventHandler<MobEventArgs> OnMobKilled;
	public static event EventHandler<ItemEventArgs> OnItemPicked;
	public static event EventHandler<QuestEventArgs> OnActivateQuest;
	public static event EventHandler<QuestEventArgs> OnDeliverQuest;
	
	public static void ItemPicked( string tag ) {
		Invoke( OnItemPicked, new ItemEventArgs( tag ) );
	}

	public static void KilledMob( string tag ) {
		Invoke( OnMobKilled, new MobEventArgs( tag ) );
	}

	public static void ActivateQuest( BaseQuest quest ) {
		Invoke( OnActivateQuest, new QuestEventArgs( quest ) );
	}

	public static void DeliverQuest( BaseQuest quest ) {
		Invoke( OnDeliverQuest, new QuestEventArgs( quest ) );
	}

	private static void Invoke( Delegate evt, EventArgs args ) {
		if ( evt != null ) {
			evt.DynamicInvoke( null, args );
		}
	}
}
