using OWML.Utils;
using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.RoastingSync.Events
{
	internal class MarshmallowEventEvent : QSBEvent<EnumMessage<MarshmallowEventType>>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.MarshmallowEvent;

		public override void SetupListener() => GlobalMessenger<MarshmallowEventType>.AddListener(EventNames.QSBMarshmallowEvent, Handler);
		public override void CloseListener() => GlobalMessenger<MarshmallowEventType>.RemoveListener(EventNames.QSBMarshmallowEvent, Handler);

		private void Handler(MarshmallowEventType type) => SendEvent(CreateMessage(type));

		private EnumMessage<MarshmallowEventType> CreateMessage(MarshmallowEventType type) => new EnumMessage<MarshmallowEventType>
		{
			AboutId = LocalPlayerId,
			EnumValue = type
		};

		public override void OnReceiveRemote(bool server, EnumMessage<MarshmallowEventType> message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var marshmallow = QSBPlayerManager.GetPlayer(message.AboutId).Marshmallow;
			if (marshmallow == null)
			{
				DebugLog.ToConsole($"Warning - Marshmallow is null for player {message.AboutId}.", OWML.Common.MessageType.Warning);
				return;
			}

			switch (message.EnumValue)
			{
				case MarshmallowEventType.Burn:
					marshmallow.Burn();
					break;
				case MarshmallowEventType.Extinguish:
					marshmallow.Extinguish();
					break;
				case MarshmallowEventType.Remove:
					marshmallow.RemoveMallow();
					break;
				case MarshmallowEventType.Replace:
					marshmallow.SpawnMallow();
					break;
				case MarshmallowEventType.Shrivel:
					marshmallow.Shrivel();
					break;
				case MarshmallowEventType.Toss:
					TossMarshmallow(message.AboutId);
					break;
			}
		}

		private void TossMarshmallow(uint playerId)
		{
			var player = QSBPlayerManager.GetPlayer(playerId);
			var stick = player.RoastingStick;
			var stickTip = stick.transform.GetChild(0);

			var mallowPrefab = Resources.FindObjectsOfTypeAll<RoastingStickController>().First().GetValue<GameObject>("_mallowBodyPrefab");

			var tossedMallow = Object.Instantiate(mallowPrefab, stickTip.position, stickTip.rotation);
			var rigidbody = tossedMallow.GetComponent<OWRigidbody>();
			if (player.Campfire == null)
			{
				DebugLog.DebugWrite($"Error - Campfire for {playerId} is null.", OWML.Common.MessageType.Error);
				return;
			}

			rigidbody.SetVelocity(player.Campfire.AttachedObject.GetAttachedOWRigidbody(false).GetPointVelocity(stickTip.position) + (stickTip.forward * 3f));
			rigidbody.SetAngularVelocity(stickTip.right * 10f);
			if (player.Marshmallow == null)
			{
				DebugLog.DebugWrite($"Error - Marshmallow for {playerId} is null.", OWML.Common.MessageType.Error);
				return;
			}

			tossedMallow.GetComponentInChildren<MeshRenderer>().material.color = player.Marshmallow._burntColor;
		}
	}
}
