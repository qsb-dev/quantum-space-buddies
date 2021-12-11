using System;
using JetBrains.Annotations;
using OWML.Utils;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.Events
{
	public abstract class QSBEvent<T> : IQSBEvent where T : PlayerMessage, new()
	{
		public uint LocalPlayerId => QSBPlayerManager.LocalPlayerId;

		[UsedImplicitly]
		private Type _messageType => typeof(T);

		[UsedImplicitly]
		private readonly int _msgType;

		protected QSBEvent()
		{
			_msgType = QSBEventManager._eventList.Count;
		}

		public abstract void SetupListener();
		public abstract void CloseListener();

		[UsedImplicitly]
		public virtual void OnReceiveRemote(bool isHost, T message) { }

		[UsedImplicitly]
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
				if (message.OnlySendToHost)
				{
					message.ForId = 0;
				}
				new QSBEventRelay
				{
					To = message.ForId,
					Event = this,
					Message = message
				}.Send();
			});

		/// <summary>
		/// Checks whether the message should be processed by the executing client.
		/// </summary>
		/// <returns>True if the message should be processed.</returns>
		[UsedImplicitly]
		public virtual bool CheckMessage(T message)
			=> !RequireWorldObjectsReady || WorldObjectManager.AllObjectsReady;
	}

	internal class QSBEventRelay : QSBMessage
	{
		public IQSBEvent Event;
		public PlayerMessage Message;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			var msgType = Event.GetValue<int>("_msgType");
			writer.Write(msgType);
			Message.Serialize(writer);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			var msgType = reader.ReadInt32();
			Event = QSBEventManager._eventList[msgType];
			var messageType = Event.GetValue<Type>("_messageType");
			Message = (PlayerMessage)Activator.CreateInstance(messageType);
			Message.Deserialize(reader);
		}

		public override bool ShouldReceive => Event.Invoke<bool>("CheckMessage", Message);
		public override void OnReceiveRemote() => Event.Invoke("OnReceiveRemote", QSBCore.IsHost, Message);
		public override void OnReceiveLocal() => Event.Invoke("OnReceiveLocal", QSBCore.IsHost, Message);
	}
}
