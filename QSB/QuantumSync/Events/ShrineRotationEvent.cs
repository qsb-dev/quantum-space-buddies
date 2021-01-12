using QSB.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.QuantumSync.Events
{
	class ShrineRotationEvent : QSBEvent<ShrineRotationMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.ShrineRotation;

		public override void SetupListener() => GlobalMessenger<Quaternion>.AddListener(EventNames.QSBMoonStateChange, Handler);
		public override void CloseListener() => GlobalMessenger<Quaternion>.RemoveListener(EventNames.QSBMoonStateChange, Handler);

		private void Handler(Quaternion rotation) => SendEvent(CreateMessage(rotation));

		private ShrineRotationMessage CreateMessage(Quaternion rotation) => new ShrineRotationMessage
		{
			AboutId = LocalPlayerId,
			Rotation = rotation
		};

		public override void OnReceiveRemote(bool server, ShrineRotationMessage message)
		{
			if (!QSBCore.HasWokenUp)
			{
				return;
			}
			Resources.FindObjectsOfTypeAll<QuantumShrine>().First().transform.rotation = message.Rotation;
		}
	}
}
