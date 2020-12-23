using QSB.Events;
using QSB.WorldSync;
using System.Reflection;
using UnityEngine;

namespace QSB.QuantumSync.Events
{
	public class SocketStateChangeEvent : QSBEvent<SocketStateChangeMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.SocketStateChange;

		public override void SetupListener() => GlobalMessenger<int, int, Quaternion>.AddListener(EventNames.QSBSocketStateChange, Handler);
		public override void CloseListener() => GlobalMessenger<int, int, Quaternion>.RemoveListener(EventNames.QSBSocketStateChange, Handler);

		private void Handler(int objid, int socketid, Quaternion localRotation) => SendEvent(CreateMessage(objid, socketid, localRotation));

		private SocketStateChangeMessage CreateMessage(int objid, int socketid, Quaternion localRotation) => new SocketStateChangeMessage
		{
			AboutId = LocalPlayerId,
			ObjectId = objid,
			SocketId = socketid,
			LocalRotation = localRotation
		};

		public override void OnReceiveRemote(bool server, SocketStateChangeMessage message)
		{
			if (!QSBCore.HasWokenUp)
			{
				return;
			}
			var obj = QSBWorldSync.GetWorldObject<QSBSocketedQuantumObject>(message.ObjectId).AttachedObject;
			var socket = QSBWorldSync.GetWorldObject<QSBQuantumSocket>(message.SocketId).AttachedSocket;
			obj.GetType().GetMethod("MoveToSocket", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(obj, new object[] { socket });
			obj.transform.localRotation = message.LocalRotation;
		}
	}
}