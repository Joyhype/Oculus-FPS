using UnityEngine;
using System.Collections;

public class PlayerQuestHandler : MonoBehaviour {

	private BaseQuest quest;

	// Use this for initialization
	void Start () {
		InformationHandler.OnActivateQuest += InformationHandler_OnActivateQuest;
	}

	private void InformationHandler_OnActivateQuest( object sender, InformationHandler.QuestEventArgs e ) {
		quest = CopyComponent( e.Quest, gameObject );
		quest.Activate();
	}


	// Update is called once per frame
	void Update () {
	
	}

	T CopyComponent<T>( T original, GameObject destination ) where T : Component {
		System.Type type = original.GetType();
		Component copy = destination.AddComponent( type );
		System.Reflection.FieldInfo[] fields = type.GetFields();
		foreach ( System.Reflection.FieldInfo field in fields ) {
			field.SetValue( copy, field.GetValue( original ) );
		}
		return copy as T;
	}
}
