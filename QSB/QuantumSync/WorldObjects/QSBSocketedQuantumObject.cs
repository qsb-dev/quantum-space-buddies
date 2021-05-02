using OWML.Common;
using QSB.Player;
using QSB.QuantumSync.Events;
using QSB.Utility;
using QSB.WorldSync;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBSocketedQuantumObject : QSBQuantumObject<SocketedQuantumObject>
	{
		public Text DebugBoxText;

		public override void Init(SocketedQuantumObject quantumObject, int id)
		{
			ObjectId = id;
			AttachedObject = quantumObject;
			base.Init(quantumObject, id);
			if (QSBCore.DebugMode)
			{
				DebugBoxText = DebugBoxManager.CreateBox(AttachedObject.transform, 0, ObjectId.ToString()).GetComponent<Text>();
			}
		}

		public override void OnRemoval()
		{
			base.OnRemoval();
			if (DebugBoxText != null)
			{
				Object.Destroy(DebugBoxText.gameObject);
			}
		}

		public void MoveToSocket(SocketStateChangeMessage message)
		{
			var qsbSocket = QSBWorldSync.GetWorldFromId<QSBQuantumSocket>(message.SocketId);
			if (qsbSocket == null)
			{
				DebugLog.ToConsole($"Couldn't find socket id {message.SocketId}", MessageType.Error);
				return;
			}
			var socket = qsbSocket.AttachedObject;
			if (socket == null)
			{
				DebugLog.ToConsole($"QSBSocket id {message.SocketId} has no attached socket.", MessageType.Error);
				return;
			}

			var wasEntangled = AttachedObject.IsPlayerEntangled();
			var component = Locator.GetPlayerTransform().GetComponent<OWRigidbody>();
			var location = new RelativeLocationData(Locator.GetPlayerTransform().GetComponent<OWRigidbody>(), AttachedObject.transform);

			AttachedObject.GetType().GetMethod("MoveToSocket", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(AttachedObject, new object[] { socket });

			if (wasEntangled)
			{
				component.MoveToRelativeLocation(location, AttachedObject.transform);
			}

			if (QuantumManager.Shrine != AttachedObject)
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