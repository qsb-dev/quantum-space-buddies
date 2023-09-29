using Mirror;
using QSB.ConversationSync.WorldObjects;
using QSB.Messaging;
using System.Linq;

namespace QSB.ConversationSync.Messages;

public class RemoteDialogueInitialStateMessage : QSBWorldObjectMessage<QSBRemoteDialogueTrigger>
{
	private bool _inRemoteDialogue;
	private bool[] _activatedDialogues;
	private int _dialogueIndex;
	private bool _colliderEnabled;

	public RemoteDialogueInitialStateMessage(RemoteDialogueTrigger trigger)
	{
		_inRemoteDialogue = trigger._inRemoteDialogue;
		_activatedDialogues = trigger._activatedDialogues;
		_dialogueIndex = trigger._listDialogues
			.IndexOf(x => x.dialogue == trigger._activeRemoteDialogue);
		_colliderEnabled = trigger._collider.enabled;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(_inRemoteDialogue);
		writer.Write(_activatedDialogues);
		writer.Write(_dialogueIndex);
		writer.Write(_colliderEnabled);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		_inRemoteDialogue = reader.Read<bool>();
		_activatedDialogues = reader.Read<bool[]>();
		_dialogueIndex = reader.Read<int>();
		_colliderEnabled = reader.Read<bool>();
	}

	public override void OnReceiveRemote()
	{
		var trigger = WorldObject.AttachedObject;
		trigger._activatedDialogues = _activatedDialogues;
		trigger._inRemoteDialogue = _inRemoteDialogue;
		trigger._activeRemoteDialogue = trigger._listDialogues.ElementAtOrDefault(_dialogueIndex).dialogue;
		trigger._collider.enabled = _colliderEnabled;
	}
}