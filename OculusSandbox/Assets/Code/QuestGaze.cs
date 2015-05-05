using UnityEngine;

public class QuestGaze : MonoBehaviour {

	private Vector3 originalScale;

	private bool isVisible = false;
	private bool ignore = false;

	private BaseQuest quest;
	private bool rejectGaze = false;

	// Use this for initialization
	void Start() {
		originalScale = transform.localScale;

		quest = GetComponent<BaseQuest>();

		MasterOfKnowledge.OnActivateQuest += MasterOfKnowledge_OnActivateQuest;
		MasterOfKnowledge.OnDeliverQuest += MasterOfKnowledge_OnDeliverQuest;
	}

	private void MasterOfKnowledge_OnDeliverQuest( object sender, QuestEventArgs e ) {
		rejectGaze = false;
	}

	private void MasterOfKnowledge_OnActivateQuest( object sender, QuestEventArgs e ) {
		rejectGaze = e.Quest != quest;
	}

	void OnGaze() {
		if ( ignore ) return;

		iTween.StopByName( gameObject, "down" );
		iTween.ScaleTo( gameObject, iTween.Hash( "name", "up", "scale", originalScale * 1.5f, "time", 0.5f ) );
	}

	void OnLostGaze() {
		if ( ignore ) return;

		iTween.StopByName( gameObject, "up" );
		iTween.ScaleTo( gameObject, iTween.Hash( "name", "down", "scale", originalScale, "time", 0.5f ) );
	}

	void OnGazed() {
		if ( ignore ) return;

		if ( !isVisible ) {
			if ( rejectGaze ) {
				DoRejectShake();
			} else {
				ActivateQuest();
			}
		} else {
			if ( quest.IsDone ) {
				DeliverQuest();
			} else {
				DoRejectShake();
			}
		}
	}

	private void ActivateQuest() {
		MasterOfKnowledge.ActivateQuest( quest );
		iTween.RotateBy( gameObject, new Vector3( 0, 0, 0.5f ), 1f );
		isVisible = true;
	}

	private void DeliverQuest() {
		MasterOfKnowledge.DeliverQuest( quest );

		iTween.ScaleTo( gameObject, new Vector3(), 1f );
		iTween.RotateBy( gameObject, iTween.Hash( "amount", new Vector3( 0, 2.5f ), "time", 1f, "easetype", iTween.EaseType.linear ) );

		ignore = true;
	}

	private void DoRejectShake() {
		iTween.ShakePosition( gameObject, new Vector3( 0.75f, 0, 0 ), 0.5f );
	}
}
