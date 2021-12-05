using OWML.Common;
using QSB.Events;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.QuantumSync.Events
{
	public class SocketStateChangeEvent : QSBEvent<SocketStateChangeMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<int, int, Quaternion>.AddListener(EventNames.QSBSocketStateChange, Handler);
		public override void CloseListener() => GlobalMessenger<int, int, Quaternion>.RemoveListener(EventNames.QSBSocketStateChange, Handler);

		private void Handler(int objid, int socketid, Quaternion localRotation) => SendEvent(CreateMessage(objid, socketid, localRotation));

		private SocketStateChangeMessage CreateMessage(int objid, int socketid, Quaternion localRotation) => new()
		{
			AboutId = LocalPlayerId,
			ObjectId = objid,
			SocketId = socketid,
			LocalRotation = localRotation
		};

		public override void OnReceiveRemote(bool server, SocketStateChangeMessage message)
		{
			var obj = QSBWorldSync.GetWorldFromId<QSBSocketedQuantumObject>(message.ObjectId);
			if (obj.ControllingPlayer != message.FromId)
			{
				DebugLog.ToConsole($"Error - Got SocketStateChangeEvent for {obj.Name} from {message.FromId}, but it's currently controlled by {obj.ControllingPlayer}!", MessageType.Error);
				return;
			}

			obj.MoveToSocket(message);
		}
	}
}