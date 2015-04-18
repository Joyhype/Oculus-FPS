using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectQuest : BaseQuest {

	public string[] ItemTags;
	public int[] ItemCounts;

	private Dictionary<string, int> items;

	// Use this for initialization
	void Start() {
		foreach ( var item in ItemTags ) {
			items.Add( item, 0 );
		}
	}

	public override void OnItemPicked( object sender, InformationHandler.ItemEventArgs args ) {
		if ( IsActive ) {
			if ( items.ContainsKey( args.Tag ) ) {
				items[args.Tag]++;
			}

			CheckConditions();
		}
	}

	private void CheckConditions() {
		bool completedAll = true;
		for ( int i = 0; i < ItemTags.Length; i++ ) {
			if ( items[ItemTags[i]] < ItemCounts[i] ) {
				completedAll = false;
				break;
			}
		}

		IsDone = completedAll;
	}
}
