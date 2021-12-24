using QSB.ClientServerStateSync;
using QSB.Events;
using UnityEngine;

namespace QSB.StatueSync.Messages
{
	internal class StartStatueEvent : QSBEvent<StartStatueMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener()
			=> GlobalMessenger<Vector3, Quaternion, float>.AddListener(EventNames.QSBStartStatue, Handler);

		public override void CloseListener()
			=> GlobalMessenger<Vector3, Quaternion, float>.RemoveListener(EventNames.QSBStartStatue, Handler);

		private void Handler(Vector3 position, Quaternion rotation, float degrees)
			=> SendEvent(CreateMessage(position, rotation, degrees));

		private StartStatueMessage CreateMessage(Vector3 position, Quaternion rotation, float degrees) => new()
		{
			AboutId = LocalPlayerId,
			PlayerPosition = position,
			PlayerRotation = rotation,
			CameraDegrees = degrees
		};

		public override void OnReceiveLocal(bool server, StartStatueMessage message)
		{
			if (!QSBCore.IsHost)
			{
				return;
			}

			ServerStateManager.Instance.SendChangeServerStateMessage(ServerState.InStatueCutscene);
		}

		public override void OnReceiveRemote(bool server, StartStatueMessage message)
		{
			StatueManager.Instance.BeginSequence(message.PlayerPosition, message.PlayerRotation, message.CameraDegrees);

			if (!QSBCore.IsHost)
			{
				return;
			}

			ServerStateManager.Instance.SendChangeServerStateMessage(ServerState.InStatueCutscene);
		}
	}
}
