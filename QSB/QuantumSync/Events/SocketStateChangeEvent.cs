using QSB.Events;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;
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
			var socket = QSBWorldSync.GetWorldObject<QSBQuantumSocket>(message.SocketId).AttachedObject;
			obj.GetType().GetMethod("MoveToSocket", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(obj, new object[] { socket });
			if ((QuantumManager.Instance.Shrine as SocketedQuantumObject) != obj)
			{
				obj.transform.localRotation = message.LocalRotation;
			}
			else
			{
				var playerToShrine = QSBPlayerManager.GetPlayer(message.FromId).Body.transform.position - obj.transform.position;
				var projectOnPlace = Vector3.ProjectOnPlane(playerToShrine, obj.transform.up);
				var angle = OWMath.Angle(obj.transform.forward, projectOnPlace, obj.transform.up);
				angle = OWMath.RoundToNearestMultiple(angle, 120f);
				obj.transform.rotation = Quaternion.AngleAxis(angle, obj.transform.up) * obj.transform.rotation;
			}
		}
	}
}