using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KillQuest : BaseQuest {

	public string[] MobTags;
	public int[] MobCounts;

	private Dictionary<string, int> mobs;

	// Use this for initialization
	void Start() {
		mobs = new Dictionary<string, int>();

		foreach ( var item in MobTags ) {
			mobs.Add( item, 0 );
		}
	}

	public override void OnMobKilled( object sender, InformationHandler.MobEventArgs args ) {
		if ( IsActive ) {
			if ( mobs.ContainsKey( args.Tag ) ) {
				mobs[args.Tag]++;
			}

			CheckConditions();
		}
	}

	private void CheckConditions() {
		bool completedAll = true;
		for ( int i = 0; i < MobTags.Length; i++ ) {
			if ( mobs[MobTags[i]] < MobCounts[i] ) {
				completedAll = false;
				break;
			}
		}

		IsDone = completedAll;
	}
}
