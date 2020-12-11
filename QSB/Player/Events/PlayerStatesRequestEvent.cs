using OWML.ModHelper.Events;
using QSB.EventsCore;
using QSB.Messaging;
using QSB.SectorSync;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Player.Events
{
	public class PlayerStatesRequestEvent : QSBEvent<PlayerMessage>
	{
		public override EventType Type => EventType.PlayerStatesRequest;

		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBPlayerStatesRequest, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBPlayerStatesRequest, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private PlayerMessage CreateMessage() => new PlayerMessage
		{
			AboutId = LocalPlayerId
		};

		public override void OnReceiveRemote(bool server, PlayerMessage message)
		{
			if (!server)
			{
				return;
			}
			DebugLog.DebugWrite($"Get state request from {message.FromId}");
			GlobalMessenger.FireEvent(EventNames.QSBServerSendPlayerStates);
			foreach (var item in QSBPlayerManager.GetSyncObjects<TransformSync.TransformSync>()
				.Where(x => x != null && x.IsReady && x.ReferenceSector != null))
			{
				GlobalMessenger<uint, QSBSector>.FireEvent(EventNames.QSBSectorChange, item.NetId.Value, item.ReferenceSector);
			}

			foreach (var condition in QSBWorldSync.DialogueConditions)
			{
				DebugLog.DebugWrite($"SENDING STATE OF CONDITION {condition.Key}");
				GlobalMessenger<string, bool>.FireEvent(EventNames.DialogueCondition, condition.Key, condition.Value);
			}
		}
	}
}