using UnityEngine;
using System.Collections;

public class KillQuest : SingleQuest {

	protected override void OnMobKilled( object sender, MobEventArgs e ) {
		CheckTag( e.Tag );
	}

	void OnGUI() {
		if ( !IsActive ) return;

		float offset = 100;

		GUI.color = Color.black;
		for ( int i = 0; i < Tags.Length; i++ ) {
			GUI.Label( new Rect( 25, 25 + ( i * offset ), 250, 100 ), string.Format( "{0}: {1}/{2}", Tags[i], Objects[Tags[i]], Amounts[i] ) );
		}
		GUI.color = Color.white;
	}
}
