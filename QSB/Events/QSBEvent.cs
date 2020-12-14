using QSB.Messaging;
using QSB.Player;
using QSB.TransformSync;
using QuantumUNET;

namespace QSB.Events
{
	public abstract class QSBEvent<T> : IQSBEvent where T : PlayerMessage, new()
	{
		public abstract EventType Type { get; }
        public uint LocalPlayerId => QSBPlayerManager.LocalPlayerId;

		private readonly MessageHandler<T> _eventHandler;

		protected QSBEvent()
		{
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
			QSBCore.Helper.Events.Unity.RunWhen(() => PlayerTransformSync.LocalInstance != null, () => Send(message));
		}

		private void Send(T message)
		{
			if (QSBNetworkServer.active)
			{
				_eventHandler.SendToAll(message);
			}
			else
			{
				_eventHandler.SendToServer(message);
			}
		}

		private void OnReceive(bool isServer, T message)
		{
			if (isServer
				&& !message.OnlySendToServer 
				&& message.FromId != QSBPlayerManager.LocalPlayerId)
			{
				_eventHandler.SendToAll(message);
			}
			if (message.FromId == QSBPlayerManager.LocalPlayerId ||
				QSBPlayerManager.IsBelongingToLocalPlayer(message.AboutId))
			{
				OnReceiveLocal(isServer, message);
				return;
			}

			OnReceiveRemote(isServer, message);
		}
	}
}