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
