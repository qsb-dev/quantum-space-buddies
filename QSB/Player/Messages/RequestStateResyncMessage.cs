using OWML.Common;
using QSB.ClientServerStateSync;
using QSB.ClientServerStateSync.Messages;
using QSB.ConversationSync.Messages;
using QSB.LogSync.Messages;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.Player.Messages
{
	// Can be sent by any client (including host) to signal they want latest worldobject, player, and server information
	public class RequestStateResyncMessage : QSBMessage
	{
		/// <summary>
		/// set to true when we send this, and false when we receive a player info message back. <br/>
		/// this prevents message spam a bit.
		/// </summary>
		internal static bool _waitingForEvent;

		/// <summary>
		/// used instead of QSBMessageManager.Send to do the extra check
		/// </summary>
		public void Send()
		{
			if (_waitingForEvent)
			{
				return;
			}

			_waitingForEvent = true;
			QSBMessageManager.Send(this);
		}

		public override void OnReceiveLocal()
		{
			QSBCore.UnityEvents.FireInNUpdates(() =>
			{
				if (_waitingForEvent)
				{
					if (QSBPlayerManager.PlayerList.Count > 1)
					{
						DebugLog.ToConsole($"Did not receive PlayerInformationEvent in time. Setting _waitingForEvent to false.", MessageType.Info);
					}

					_waitingForEvent = false;
				}
			}, 60);
		}

		public override void OnReceiveRemote()
		{
			// if host, send worldobject and server states
			if (QSBCore.IsHost)
			{
				new ServerStateMessage(ServerStateManager.Instance.GetServerState()) { To = From }.Send();
				new PlayerInformationMessage { To = From }.Send();

				if (QSBWorldSync.AllObjectsReady)
				{
					QSBWorldSync.DialogueConditions.ForEach(condition
						=> new DialogueConditionMessage(condition.Key, condition.Value) { To = From }.Send());

					QSBWorldSync.ShipLogFacts.ForEach(fact
						=> new RevealFactMessage(fact.Id, fact.SaveGame, false) { To = From }.Send());
				}
			}
			// if client, send player and client states
			else
			{
				new PlayerInformationMessage { To = From }.Send();
			}

			if (QSBWorldSync.AllObjectsReady)
			{
				foreach (var worldObject in QSBWorldSync.GetWorldObjects())
				{
					worldObject.SendResyncInfo(From);
				}
			}
		}
	}
}
