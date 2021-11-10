using QSB.Anglerfish.WorldObjects;
using QSB.Events;
using QSB.Player;
using QSB.WorldSync;
using UnityEngine;
using static AnglerfishController;
using EventType = QSB.Events.EventType;

namespace QSB.Anglerfish.Events
{
	public class AnglerChangeStateEvent : QSBEvent<AnglerChangeStateMessage>
	{
		public override EventType Type => EventType.AnglerChangeState;
		public override void SetupListener() => GlobalMessenger<QSBAngler>.AddListener(EventNames.QSBAnglerChangeState, Handler);
		public override void CloseListener() => GlobalMessenger<QSBAngler>.RemoveListener(EventNames.QSBAnglerChangeState, Handler);

		private void Handler(QSBAngler qsbAngler) =>
			SendEvent(new AnglerChangeStateMessage
			{
				ObjectId = qsbAngler.ObjectId,
				EnumValue = qsbAngler.AttachedObject._currentState,
				targetId = TargetToId(qsbAngler.targetTransform),
				localDisturbancePos = qsbAngler.AttachedObject._localDisturbancePos
			});

		public override void OnReceiveLocal(bool isHost, AnglerChangeStateMessage message) => OnReceive(isHost, message);
		public override void OnReceiveRemote(bool isHost, AnglerChangeStateMessage message) => OnReceive(isHost, message);

		private static void OnReceive(bool isHost, AnglerChangeStateMessage message)
		{
			var qsbAngler = QSBWorldSync.GetWorldFromId<QSBAngler>(message.ObjectId);

			if (isHost)
			{
				qsbAngler.TransferAuthority(message.FromId);
			}

			qsbAngler.targetTransform = IdToTarget(message.targetId);
			qsbAngler.AttachedObject._localDisturbancePos = message.localDisturbancePos;
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
