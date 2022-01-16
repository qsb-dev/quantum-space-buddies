﻿using QSB.ClientServerStateSync;
using QSB.Messaging;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.StatueSync.Messages
{
	internal class StartStatueMessage : QSBMessage
	{
		private Vector3 PlayerPosition;
		private Quaternion PlayerRotation;
		private float CameraDegrees;

		public StartStatueMessage(Vector3 position, Quaternion rotation, float degrees)
		{
			PlayerPosition = position;
			PlayerRotation = rotation;
			CameraDegrees = degrees;
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerPosition);
			writer.Write(PlayerRotation);
			writer.Write(CameraDegrees);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			PlayerPosition = reader.ReadVector3();
			PlayerRotation = reader.ReadQuaternion();
			CameraDegrees = reader.ReadSingle();
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveLocal()
		{
			if (QSBCore.IsHost)
			{
				ServerStateManager.Instance.SendChangeServerStateMessage(ServerState.InStatueCutscene);
			}
		}

		public override void OnReceiveRemote()
		{
			if (QSBCore.IsHost)
			{
				ServerStateManager.Instance.SendChangeServerStateMessage(ServerState.InStatueCutscene);
			}

			StatueManager.Instance.BeginSequence(PlayerPosition, PlayerRotation, CameraDegrees);
		}
	}
}