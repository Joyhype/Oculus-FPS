using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SingleQuest : BaseQuest {

	public string[] Tags;
	public int[] Amounts;

	protected Dictionary<string, int> Objects = new Dictionary<string, int>();

	// Use this for initialization
	void Start () {
		foreach ( var item in Tags ) {
			Objects.Add( item, 0 );
		}
	}

	protected void CheckTag( string tag ) {
		if ( IsActive ) {
			if ( Objects.ContainsKey( tag ) ) {
				Objects[tag]++;

				CheckConditions();
			}
		}
	}

	private void CheckConditions() {
		bool completedAll = true;
		for ( int i = 0; i < Tags.Length; i++ ) {
			if ( Objects[Tags[i]] < Amounts[i] ) {
				completedAll = false;
				break;
			}
		}

		if ( completedAll ) {
			FinishQuest();
		}
	}
}
