using Cysharp.Threading.Tasks;
using OWML.Common;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Threading;
using UnityEngine;

namespace QSB.QuantumSync.WorldObjects;

public class QSBSocketedQuantumObject : QSBQuantumObject<SocketedQuantumObject>
{
	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);
		AttachedObject._randomYRotation = false;
	}

	public override string ReturnLabel()
	{
		var socket = AttachedObject.GetCurrentSocket();
		if (socket != null)
		{
			var socketObj = socket.GetWorldObject<QSBQuantumSocket>();
			return $"{base.ReturnLabel()}SocketId:{socketObj.ObjectId}";
		}
		else
		{
			return $"{base.ReturnLabel()}SocketId:NULL";
		}
	}

	public void MoveToSocket(uint playerId, int socketId, Quaternion localRotation)
	{
		var qsbSocket = socketId.GetWorldObject<QSBQuantumSocket>();
		if (qsbSocket == null)
		{
			DebugLog.ToConsole($"Couldn't find socket id {socketId}", MessageType.Error);
			return;
		}

		var socket = qsbSocket.AttachedObject;
		if (socket == null)
		{
			DebugLog.ToConsole($"QSBSocket id {socketId} has no attached socket.", MessageType.Error);
			return;
		}

		var wasEntangled = AttachedObject.IsPlayerEntangled();
		var component = Locator.GetPlayerTransform().GetComponent<OWRigidbody>();
		var location = new RelativeLocationData(Locator.GetPlayerTransform().GetComponent<OWRigidbody>(), AttachedObject.transform);

		AttachedObject.MoveToSocket(socket);

		if (wasEntangled)
		{
			component.MoveToRelativeLocation(location, AttachedObject.transform);
		}

		if (QuantumManager.Shrine != AttachedObject)
		{
			AttachedObject.transform.localRotation = localRotation;
		}
		else
		{
			var playerToShrine = QSBPlayerManager.GetPlayer(playerId).Body.transform.position - AttachedObject.transform.position;
			var projectOnPlace = Vector3.ProjectOnPlane(playerToShrine, AttachedObject.transform.up);
			var angle = OWMath.Angle(AttachedObject.transform.forward, projectOnPlace, AttachedObject.transform.up);
			angle = OWMath.RoundToNearestMultiple(angle, 120f);
			AttachedObject.transform.rotation = Quaternion.AngleAxis(angle, AttachedObject.transform.up) * AttachedObject.transform.rotation;
		}
	}
}