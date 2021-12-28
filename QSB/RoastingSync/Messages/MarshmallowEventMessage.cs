using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.RoastingSync.Messages
{
	internal class MarshmallowEventMessage : QSBEnumMessage<MarshmallowMessageType>
	{
		public MarshmallowEventMessage(MarshmallowMessageType type) => Value = type;

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			if (player.Marshmallow == null)
			{
				DebugLog.ToConsole($"Warning - Marshmallow is null for player {player.PlayerId}.", MessageType.Warning);
				return;
			}

			switch (Value)
			{
				case MarshmallowMessageType.Burn:
					player.Marshmallow.Burn();
					break;
				case MarshmallowMessageType.Extinguish:
					player.Marshmallow.Extinguish();
					break;
				case MarshmallowMessageType.Remove:
					player.Marshmallow.RemoveMallow();
					break;
				case MarshmallowMessageType.Replace:
					player.Marshmallow.SpawnMallow();
					break;
				case MarshmallowMessageType.Shrivel:
					player.Marshmallow.Shrivel();
					break;
				case MarshmallowMessageType.Toss:
					TossMarshmallow(player);
					break;
			}
		}

		private static void TossMarshmallow(PlayerInfo player)
		{
			var stick = player.RoastingStick;
			var stickTip = stick.transform.GetChild(0);

			var mallowPrefab = QSBWorldSync.GetUnityObjects<RoastingStickController>().First()._mallowBodyPrefab;

			var tossedMallow = Object.Instantiate(mallowPrefab, stickTip.position, stickTip.rotation);
			var rigidbody = tossedMallow.GetComponent<OWRigidbody>();
			if (player.Campfire == null)
			{
				DebugLog.ToConsole($"Error - Campfire for {player.PlayerId} is null.", MessageType.Error);
				return;
			}

			rigidbody.SetVelocity(player.Campfire.AttachedObject.GetAttachedOWRigidbody().GetPointVelocity(stickTip.position) + stickTip.forward * 3f);
			rigidbody.SetAngularVelocity(stickTip.right * 10f);
			if (player.Marshmallow == null)
			{
				DebugLog.ToConsole($"Error - Marshmallow for {player.PlayerId} is null.", MessageType.Error);
				return;
			}

			tossedMallow.GetComponentInChildren<MeshRenderer>().material.color = player.Marshmallow._burntColor;
		}
	}
}
