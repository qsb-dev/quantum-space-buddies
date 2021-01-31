using OWML.Common;
using QSB.Player;
using QSB.QuantumSync.Events;
using QSB.Utility;
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
			/*
			var visibilityTrackers = AttachedObject.GetValue<VisibilityTracker[]>("_visibilityTrackers");
			var visible = visibilityTrackers.Where(x => x is ShapeVisibilityTracker).Any(x => QuantumManager.IsVisibleUsingCameraFrustum(x as ShapeVisibilityTracker, false));
			if (visible)
			{
				DebugLog.DebugWrite($"Error - trying to move {AttachedObject.name} while still visible!", MessageType.Error);
			}
			*/
			var qsbSocket = QSBWorldSync.GetWorldObject<QSBQuantumSocket>(message.SocketId);
			if (qsbSocket == null)
			{
				DebugLog.DebugWrite($"Couldn't find socket id {message.SocketId}", MessageType.Error);
				return;
			}
			var socket = qsbSocket.AttachedObject;
			if (socket == null)
			{
				DebugLog.DebugWrite($"QSBSocket id {message.SocketId} has no attached socket.", MessageType.Error);
				return;
			}
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