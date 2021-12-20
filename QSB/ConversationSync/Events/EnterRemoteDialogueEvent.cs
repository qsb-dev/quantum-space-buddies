using QSB.ConversationSync.WorldObjects;
using QSB.Events;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.ConversationSync.Events
{
	internal class EnterRemoteDialogueEvent : QSBEvent<EnterRemoteDialogueMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<QSBRemoteDialogueTrigger, int, int>.AddListener(EventNames.QSBEnterRemoteDialogue, Handler);
		public override void CloseListener() => GlobalMessenger<QSBRemoteDialogueTrigger, int, int>.RemoveListener(EventNames.QSBEnterRemoteDialogue, Handler);

		private void Handler(QSBRemoteDialogueTrigger remoteTrigger, int activatedIndex, int listIndex) => SendEvent(CreateMessage(remoteTrigger, activatedIndex, listIndex));

		private EnterRemoteDialogueMessage CreateMessage(QSBRemoteDialogueTrigger remoteTrigger, int activatedIndex, int listIndex) => new()
		{
			AboutId = LocalPlayerId,
			ObjectId = remoteTrigger.ObjectId,
			ActivatedDialogueIndex = activatedIndex,
			ListDialoguesIndex = listIndex
		};

		public override void OnReceiveRemote(bool isHost, EnterRemoteDialogueMessage message)
		{
			var qsbObj = QSBWorldSync.GetWorldFromId<QSBRemoteDialogueTrigger>(message.ObjectId);
			qsbObj.RemoteEnterDialogue(message.ActivatedDialogueIndex, message.ListDialoguesIndex);
		}
	}
}
