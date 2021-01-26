using QSB.Player;
using QSB.QuantumSync.Events;
using QSB.WorldSync;
using System.Reflection;
using UnityEngine;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBSocketedQuantumObject : QSBQuantumObject<SocketedQuantumObject>
	{
		public override void Init(SocketedQuantumObject quantumObject, int id)
		{
			ObjectId = id;
			AttachedObject = quantumObject;
			base.Init(quantumObject, id);
		}

		public void MoveToSocket(SocketStateChangeMessage message)
		{
			var socket = QSBWorldSync.GetWorldObject<QSBQuantumSocket>(message.SocketId).AttachedObject;
			AttachedObject.GetType().GetMethod("MoveToSocket", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(AttachedObject, new object[] { socket });
			if ((QuantumManager.Instance.Shrine as SocketedQuantumObject) != AttachedObject)
			{
				AttachedObject.transform.localRotation = message.LocalRotation;
			}
			else
			{
				var playerToShrine = QSBPlayerManager.GetPlayer(message.FromId).Body.transform.position - AttachedObject.transform.position;
				var projectOnPlace = Vector3.ProjectOnPlane(playerToShrine, AttachedObject.transform.up);
				var angle = OWMath.Angle(AttachedObject.transform.forward, projectOnPlace, AttachedObject.transform.up);
				angle = OWMath.RoundToNearestMultiple(angle, 120f);
				AttachedObject.transform.rotation = Quaternion.AngleAxis(angle, AttachedObject.transform.up) * AttachedObject.transform.rotation;
			}
		}
	}
}