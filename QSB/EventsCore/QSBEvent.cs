﻿using OWML.ModHelper.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.TransformSync;
using QSB.Utility;
using QuantumUNET;
using System;

namespace QSB.EventsCore
{
	public abstract class QSBEvent<T> : IQSBEvent where T : PlayerMessage, new()
	{
		public abstract EventType Type { get; }
		public uint LocalPlayerId => QSBPlayerManager.LocalPlayerId;
		private readonly MessageHandler<T> _eventHandler;

		protected QSBEvent()
		{
			_eventHandler = new MessageHandler<T>(Type);
			_eventHandler.OnClientReceiveMessage += OnReceive;
			_eventHandler.OnServerReceiveMessage += OnReceive;
		}

		public abstract void SetupListener();

		public abstract void CloseListener();

		public virtual void OnReceiveRemote(bool server, T message) { }
		public virtual void OnReceiveLocal(bool server, T message) { }

		public void SendEvent(T message)
		{
			message.FromId = QSBPlayerManager.LocalPlayerId;
			QSB.Helper.Events.Unity.RunWhen(() => PlayerTransformSync.LocalInstance != null, () => Send(message));
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

		private void OnReceive(T message)
		{
			/* Uncomment for event debugging - very noisey and annoying (keeping as I don't want to have to rewrite this each time)
			if (_eventHandler.BaseEventType != EventType.ServerTime && _eventHandler.BaseEventType != EventType.Geyser)
			{
				DebugLog.DebugWrite($"message type {_eventHandler.BaseEventType} from {message.FromId}, about {message.AboutId}. isserver? {QSB.IsServer}");
			}
			*/
			if (message.FromId == QSBPlayerManager.LocalPlayerId ||
				QSBPlayerManager.IsBelongingToLocalPlayer(message.AboutId))
			{
				OnReceiveLocal(QSB.IsServer, message);
				return;
			}

			OnReceiveRemote(QSB.IsServer, message);
		}
	}
}