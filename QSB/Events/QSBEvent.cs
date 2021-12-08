using System;
using OWML.Common;
using QSB.ClientServerStateSync;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.Events;
using QSB.Player.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Components;
using QuantumUNET.Transport;

namespace QSB.Events
{
	public abstract class QSBEvent<T> : IQSBEvent where T : PlayerMessage, new()
	{
		private class Msg : QSBMessage
		{
			public QSBEvent<T> Event;
			public T Message;

			public override void Serialize(QNetworkWriter writer)
			{
				writer.Write(Event._msgType);
				Message.Serialize(writer);
			}

			public override void Deserialize(QNetworkReader reader)
			{
				Event = (QSBEvent<T>)QSBEventManager._eventList[reader.ReadInt32()];
				Message = new T();
				Message.Deserialize(reader);
			}

			public override bool ShouldReceive => Event.CheckMessage(Message);
			public override void OnReceiveLocal() => Event.OnReceiveLocal(QSBCore.IsHost, Message);
			public override void OnReceiveRemote() => Event.OnReceiveRemote(QSBCore.IsHost, Message);
		}

		public uint LocalPlayerId => QSBPlayerManager.LocalPlayerId;

		private readonly MessageHandler<T> _eventHandler;
		private readonly int _msgType;

		protected QSBEvent()
		{
			if (UnitTestDetector.IsInUnitTest)
			{
				return;
			}

			_msgType = QSBEventManager._eventList.Count;
			DebugLog.DebugWrite($"{GetType().Name} msgtype = {_msgType}");
			_eventHandler = new MessageHandler<T>(_msgType);
			// _eventHandler.OnClientReceiveMessage += message => OnReceive(false, message);
			// _eventHandler.OnServerReceiveMessage += message => OnReceive(true, message);
		}

		public abstract void SetupListener();
		public abstract void CloseListener();

		public virtual void OnReceiveRemote(bool isHost, T message) { }
		public virtual void OnReceiveLocal(bool isHost, T message) { }

		public abstract bool RequireWorldObjectsReady { get; }

		public void SendEvent(T message) => QSBCore.UnityEvents.RunWhen(
			() => PlayerTransformSync.LocalInstance != null,
			() =>
			{
				message.FromId = LocalPlayerId;
				if (QSBEventManager.ForIdOverride != uint.MaxValue)
				{
					message.ForId = QSBEventManager.ForIdOverride;
				}
				new Msg
				{
					Event = this,
					Message = message
				}.Send(message.ForId);
				// _eventHandler.SendToServer(message);
			});

		/// <summary>
		/// Checks whether the message should be processed by the executing client.
		/// </summary>
		/// <returns>True if the message should be processed.</returns>
		public virtual bool CheckMessage(T message)
			=> !RequireWorldObjectsReady || WorldObjectManager.AllObjectsReady;

		private void OnReceive(bool isServer, T message)
		{
			/* Explanation :
			 * if <isServer> is true, this message has been received on the server *server*.
			 * Therefore, we don't want to do any event handling code - that should be dealt
			 * with on the server *client* and any other client. So just forward the message
			 * onto all clients. This way, the server *server* just acts as the distribution
			 * hub for all events.
			 */

			if (isServer)
			{
				if (message.OnlySendToHost)
				{
					_eventHandler.SendToHost(message);
				}
				else if (message.ForId != uint.MaxValue)
				{
					_eventHandler.SendTo(message.ForId, message);
				}
				else
				{
					_eventHandler.SendToAll(message);
				}
				return;
			}

			if (!CheckMessage(message))
			{
				return;
			}

			if (PlayerTransformSync.LocalInstance == null || PlayerTransformSync.LocalInstance.GetComponent<QNetworkIdentity>() == null)
			{
				DebugLog.ToConsole($"Warning - Tried to handle message of type <{GetType().Name}> before localplayer was established.", MessageType.Warning);
				return;
			}

			if (QSBPlayerManager.PlayerExists(message.FromId))
			{
				var player = QSBPlayerManager.GetPlayer(message.FromId);

				if (!player.IsReady
					&& player.PlayerId != LocalPlayerId
					&& player.State is ClientState.AliveInSolarSystem or ClientState.AliveInEye or ClientState.DeadInSolarSystem
					&& this is not PlayerInformationEvent or PlayerReadyEvent)
				{
					DebugLog.ToConsole($"Warning - Got message from player {message.FromId}, but they were not ready. Asking for state resync, just in case.", MessageType.Warning);
					QSBEventManager.FireEvent(EventNames.QSBRequestStateResync);
				}
			}

			try
			{
				if (QSBPlayerManager.IsBelongingToLocalPlayer(message.FromId))
				{
					OnReceiveLocal(QSBCore.IsHost, message);
				}
				else
				{
					OnReceiveRemote(QSBCore.IsHost, message);
				}
			}
			catch (Exception ex)
			{
				DebugLog.ToConsole($"Error - Exception handling message {GetType().Name} : {ex}", MessageType.Error);
			}
		}
	}
}
