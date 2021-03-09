using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.TransformSync;
using QSB.Utility;
using QuantumUNET;
using QuantumUNET.Components;

namespace QSB.Events
{
	public abstract class QSBEvent<T> : IQSBEvent where T : PlayerMessage, new()
	{
		public abstract EventType Type { get; }
		public uint LocalPlayerId => QSBPlayerManager.LocalPlayerId;

		private readonly MessageHandler<T> _eventHandler;

		protected QSBEvent()
		{
			if (UnitTestDetector.IsInUnitTest)
			{
				return;
			}
			_eventHandler = new MessageHandler<T>(Type);
			_eventHandler.OnClientReceiveMessage += message => OnReceive(false, message);
			_eventHandler.OnServerReceiveMessage += message => OnReceive(true, message);
		}

		public abstract void SetupListener();
		public abstract void CloseListener();

		public virtual void OnReceiveRemote(bool server, T message) { }
		public virtual void OnReceiveLocal(bool server, T message) { }

		public void SendEvent(T message)
		{
			message.FromId = QSBPlayerManager.LocalPlayerId;
			QSBCore.Helper.Events.Unity.RunWhen(
				() => PlayerTransformSync.LocalInstance != null,
				() => _eventHandler.SendToServer(message));
		}

		/// <summary>
		/// Checks whether the message should be processed by the executing client/server.
		/// </summary>
		/// <returns>True if the message should be processed.</returns>
		public virtual bool CheckMessage(bool isServer, T message) => true;

		private void OnReceive(bool isServer, T message)
		{
			/* Explanation :
			 * if <isServer> is true, this message has been received on the server *server*.
			 * Therefore, we don't want to do any event handling code - that should be dealt
			 * with on the server *client* and any other client. So just forward the message
			 * onto all clients. This way, the server *server* just acts as the ditribution
			 * hub for all events.
			 */

			if (!CheckMessage(isServer, message))
			{
				return;
			}

			if (isServer)
			{
				_eventHandler.SendToAll(message);
				return;
			}

			if (message.OnlySendToServer && !QNetworkServer.active)
			{
				return;
			}

			if (PlayerTransformSync.LocalInstance == null || PlayerTransformSync.LocalInstance.GetComponent<QNetworkIdentity>() == null)
			{
				DebugLog.ToConsole($"Warning - Tried to handle message of type <{message.GetType().Name}> before localplayer was established.", MessageType.Warning);
				return;
			}

			if (message.FromId == QSBPlayerManager.LocalPlayerId ||
				QSBPlayerManager.IsBelongingToLocalPlayer(message.FromId))
			{
				OnReceiveLocal(QNetworkServer.active, message);
				return;
			}

			OnReceiveRemote(QNetworkServer.active, message);
		}
	}
}