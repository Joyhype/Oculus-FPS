using System;

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

public class QuestEventArgs : EventArgs {
	public readonly BaseQuest Quest;

	public QuestEventArgs( BaseQuest quest ) : base() {
		Quest = quest;
	}
}