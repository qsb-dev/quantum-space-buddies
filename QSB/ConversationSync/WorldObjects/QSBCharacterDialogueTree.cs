using Cysharp.Threading.Tasks;
using OWML.Utils;
using QSB.ConversationSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.ConversationSync.WorldObjects;

public class QSBCharacterDialogueTree : WorldObject<CharacterDialogueTree>
{
	public override async UniTask Init(CancellationToken ct)
	{
		QSBPlayerManager.OnRemovePlayer += OnRemovePlayer;
	}

	public override void OnRemoval()
	{
		QSBPlayerManager.OnRemovePlayer -= OnRemovePlayer;
	}

	public override void SendInitialState(uint to)
	{
		var playerId = ConversationManager.Instance.GetPlayerTalkingToTree(AttachedObject);
		if (playerId != uint.MaxValue)
		{
			this.SendMessage(new ConversationStartEndMessage(playerId, true) { To = to });
		}
		// TODO: maybe also sync the dialogue box and player box?
	}

	private void OnRemovePlayer(PlayerInfo player)
	{
		if (player.CurrentCharacterDialogueTree == this)
		{
			AttachedObject.GetInteractVolume().EnableInteraction();
			AttachedObject.RaiseEvent(nameof(CharacterDialogueTree.OnEndConversation));
			Object.Destroy(ConversationManager.Instance.BoxMappings[AttachedObject]);
			Object.Destroy(player.CurrentDialogueBox);
		}
	}
}
