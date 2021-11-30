using QSB.Anglerfish.WorldObjects;
using QSB.Events;
using QSB.Player;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Anglerfish.Events
{
	public class AnglerChangeStateEvent : QSBEvent<AnglerChangeStateMessage>
	{
		public override void SetupListener()
			=> GlobalMessenger<QSBAngler>.AddListener(EventNames.QSBAnglerChangeState, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBAngler>.RemoveListener(EventNames.QSBAnglerChangeState, Handler);

		private void Handler(QSBAngler qsbAngler) => SendEvent(CreateMessage(qsbAngler));

		private AnglerChangeStateMessage CreateMessage(QSBAngler qsbAngler) => new()
		{
			ObjectId = qsbAngler.ObjectId,
			EnumValue = qsbAngler.AttachedObject._currentState,
			TargetId = TargetToId(qsbAngler.TargetTransform),
			LocalDisturbancePos = qsbAngler.AttachedObject._localDisturbancePos
		};

		public override void OnReceiveLocal(bool isHost, AnglerChangeStateMessage message) => OnReceive(isHost, message);
		public override void OnReceiveRemote(bool isHost, AnglerChangeStateMessage message) => OnReceive(isHost, message);

		private static void OnReceive(bool isHost, AnglerChangeStateMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var qsbAngler = QSBWorldSync.GetWorldFromId<QSBAngler>(message.ObjectId);

			if (isHost)
			{
				qsbAngler.TransferAuthority(message.FromId);
			}

			qsbAngler.TargetTransform = IdToTarget(message.TargetId);
			qsbAngler.AttachedObject._localDisturbancePos = message.LocalDisturbancePos;
			qsbAngler.AttachedObject.ChangeState(message.EnumValue);
		}

		private static uint TargetToId(Transform transform)
		{
			if (transform == null)
			{
				return uint.MaxValue;
			}

			if (transform == Locator.GetShipTransform())
			{
				return uint.MaxValue - 1;
			}

			return QSBPlayerManager.LocalPlayerId;
		}

		private static Transform IdToTarget(uint id)
		{
			if (id == uint.MaxValue)
			{
				return null;
			}

			if (id == uint.MaxValue - 1)
			{
				return Locator.GetShipTransform();
			}

			return QSBPlayerManager.GetPlayer(id).Body.transform;
		}
	}
}
