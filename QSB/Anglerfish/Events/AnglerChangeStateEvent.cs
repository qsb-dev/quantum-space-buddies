using QSB.Anglerfish.WorldObjects;
using QSB.Events;
using QSB.Player;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Anglerfish.Events
{
	public class AnglerChangeStateEvent : QSBEvent<AnglerChangeStateMessage_OLD>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener()
			=> GlobalMessenger<QSBAngler>.AddListener(EventNames.QSBAnglerChangeState, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBAngler>.RemoveListener(EventNames.QSBAnglerChangeState, Handler);

		private void Handler(QSBAngler qsbAngler) => SendEvent(CreateMessage(qsbAngler));

		private AnglerChangeStateMessage_OLD CreateMessage(QSBAngler qsbAngler) => new()
		{
			ObjectId = qsbAngler.ObjectId,
			EnumValue = qsbAngler.AttachedObject._currentState,
			TargetId = TargetToId(qsbAngler.TargetTransform),
			LocalDisturbancePos = qsbAngler.AttachedObject._localDisturbancePos
		};

		public override void OnReceiveRemote(bool isHost, AnglerChangeStateMessage_OLD message)
		{
			var qsbAngler = QSBWorldSync.GetWorldFromId<QSBAngler>(message.ObjectId);
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

	public class AnglerChangeStateMessage : QSBEnumWorldObjectMessage<QSBAngler, AnglerfishController.AnglerState>
	{
		private uint TargetId;
		private Vector3 LocalDisturbancePos;

		public AnglerChangeStateMessage(QSBAngler qsbAngler)
		{
			Value = qsbAngler.AttachedObject._currentState;
			TargetId = TargetToId(qsbAngler.TargetTransform);
			LocalDisturbancePos = qsbAngler.AttachedObject._localDisturbancePos;
		}

		public AnglerChangeStateMessage() { }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			TargetId = reader.ReadUInt32();
			LocalDisturbancePos = reader.ReadVector3();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(TargetId);
			writer.Write(LocalDisturbancePos);
		}

		public override void OnReceiveRemote()
		{
			WorldObject.TargetTransform = IdToTarget(TargetId);
			WorldObject.AttachedObject._localDisturbancePos = LocalDisturbancePos;
			WorldObject.AttachedObject.ChangeState(Value);
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
