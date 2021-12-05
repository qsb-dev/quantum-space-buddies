using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.SaveSync.Events
{
	internal class RequestGameStateEvent : QSBEvent<PlayerMessage>
	{
		public override bool RequireWorldObjectsReady => false;

		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBRequestGameDetails, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBRequestGameDetails, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private PlayerMessage CreateMessage() => new()
		{
			AboutId = LocalPlayerId,
			OnlySendToHost = true
		};

		public override void OnReceiveRemote(bool isHost, PlayerMessage message) => QSBEventManager.FireEvent(EventNames.QSBGameDetails, message.FromId);
	}
}
