using UnityEngine;

public class LootGrabber : MonoBehaviour {

	void OnTriggerEnter( Collider other ) {
		if ( other.gameObject.layer == LayerMask.NameToLayer( "Collectible" ) ) {
			MasterOfKnowledge.ItemPicked( other.gameObject.tag );
		}

		other.gameObject.BroadcastMessage( "OnPickup" );
	}
}
