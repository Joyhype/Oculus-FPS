using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KillCollectQuest : BaseQuest {

	public string[] CollectTags;
	public string[] KillTags;

	public int[] CollectAmounts;
	public int[] KillAmounts;

	protected Dictionary<string, int> collectObjects = new Dictionary<string, int>();
	protected Dictionary<string, int> killObjects = new Dictionary<string, int>();

	// Use this for initialization
	void Start() {
		foreach ( var item in CollectTags ) {
			collectObjects.Add( item, 0 );
		}

		foreach ( var item in KillTags ) {
			killObjects.Add( item, 0 );
		}
	}

	protected void CheckTag( string tag ) {
		if ( IsActive ) {
			if ( collectObjects.ContainsKey( tag ) ) {
				collectObjects[tag]++;
				CheckConditions();
			}

			if ( killObjects.ContainsKey( tag ) ) {
				killObjects[tag]++;
				CheckConditions();
			}
		}
	}

	private void CheckConditions() {
		bool completedAll = true;

		for ( int i = 0; i < CollectTags.Length; i++ ) {
			if ( killObjects[CollectTags[i]] < KillAmounts[i] ) {
				completedAll = false;
				break;
			}
		}
		
		for ( int i = 0; i < KillTags.Length; i++ ) {
			if ( killObjects[KillTags[i]] < KillAmounts[i] ) {
				completedAll = false;
				break;
			}
		}

		if ( completedAll ) {
			FinishQuest();
		}
	}
}
