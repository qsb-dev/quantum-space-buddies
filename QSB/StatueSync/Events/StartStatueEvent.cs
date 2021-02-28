using QSB.Events;
using UnityEngine;

namespace QSB.StatueSync.Events
{
	class StartStatueEvent : QSBEvent<StartStatueMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.StartStatue;

		public override void SetupListener() 
			=> GlobalMessenger<Vector3, Quaternion, float>.AddListener(EventNames.QSBStartStatue, Handler);

		public override void CloseListener() 
			=> GlobalMessenger<Vector3, Quaternion, float>.RemoveListener(EventNames.QSBStartStatue, Handler);

		private void Handler(Vector3 position, Quaternion rotation, float degrees) 
			=> SendEvent(CreateMessage(position, rotation, degrees));

		private StartStatueMessage CreateMessage(Vector3 position, Quaternion rotation, float degrees) => new StartStatueMessage
		{
			AboutId = LocalPlayerId,
			PlayerPosition = position,
			PlayerRotation = rotation,
			CameraDegrees = degrees
		};

		public override void OnReceiveRemote(bool server, StartStatueMessage message) 
			=> StatueManager.Instance.BeginSequence(message.PlayerPosition, message.PlayerRotation, message.CameraDegrees);
	}
}
