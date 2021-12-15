using System.Linq;
using OWML.Utils;
using QSB.CampfireSync.WorldObjects;
using QSB.ClientServerStateSync;
using QSB.Events;
using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QSB.OrbSync;
using QSB.OrbSync.TransformSync;
using QSB.OrbSync.WorldObjects;
using QSB.QuantumSync.WorldObjects;
using QSB.Tools.TranslatorTool.TranslationSync;
using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;
using QSB.TornadoSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.Player.Events
{
	// Can be sent by any client (including host) to signal they want latest worldobject, player, and server infomation
	public class RequestStateResyncEvent : QSBEvent<PlayerMessage>
	{
		public static bool _waitingForEvent;

		public override bool RequireWorldObjectsReady => false;

		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBRequestStateResync, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBRequestStateResync, Handler);

		private void Handler()
		{
			if (_waitingForEvent)
			{
				return;
			}

			_waitingForEvent = true;
			SendEvent(CreateMessage());
		}

		private PlayerMessage CreateMessage() => new()
		{
			AboutId = LocalPlayerId
		};

		public override void OnReceiveLocal(bool isHost, PlayerMessage message)
		{
			QSBCore.UnityEvents.FireInNUpdates(() =>
			{
				if (_waitingForEvent)
				{
					DebugLog.ToConsole($"Did not receive PlayerInformationEvent in time. Setting _waitingForEvent to false.", OWML.Common.MessageType.Info);
					_waitingForEvent = false;
				}
			}, 60);
		}

		public override void OnReceiveRemote(bool isHost, PlayerMessage message)
		{
			// send response only to the requesting client
			QSBEventManager.ForIdOverride = message.FromId;
			try
			{
				// if host, send worldobject and server states
				if (isHost)
				{
					QSBEventManager.FireEvent(EventNames.QSBServerState, ServerStateManager.Instance.GetServerState());
					QSBEventManager.FireEvent(EventNames.QSBPlayerInformation);

					if (WorldObjectManager.AllObjectsReady)
					{
						SendWorldObjectInfo();
					}
				}
				// if client, send player and client states
				else
				{
					QSBEventManager.FireEvent(EventNames.QSBPlayerInformation);
				}

				if (WorldObjectManager.AllObjectsReady)
				{
					SendAuthorityObjectInfo();
				}
			}
			finally
			{
				QSBEventManager.ForIdOverride = uint.MaxValue;
			}
		}

		private void SendWorldObjectInfo()
		{
			QSBWorldSync.DialogueConditions.ForEach(condition
				=> QSBEventManager.FireEvent(EventNames.DialogueConditionChanged, condition.Key, condition.Value));

			QSBWorldSync.ShipLogFacts.ForEach(fact
				=> QSBEventManager.FireEvent(EventNames.QSBRevealFact, fact.Id, fact.SaveGame, false));

			foreach (var wallText in QSBWorldSync.GetWorldObjects<QSBWallText>().Where(x => x.AttachedObject.GetValue<bool>("_initialized") && x.AttachedObject.GetNumTextBlocks() > 0))
			{
				wallText.GetTranslatedIds().ForEach(id
					=> QSBEventManager.FireEvent(EventNames.QSBTextTranslated, NomaiTextType.WallText, wallText.ObjectId, id));
			}

			foreach (var computer in QSBWorldSync.GetWorldObjects<QSBComputer>().Where(x => x.AttachedObject.GetValue<bool>("_initialized") && x.AttachedObject.GetNumTextBlocks() > 0))
			{
				computer.GetTranslatedIds().ForEach(id
					=> QSBEventManager.FireEvent(EventNames.QSBTextTranslated, NomaiTextType.Computer, computer.ObjectId, id));
			}

			foreach (var vesselComputer in QSBWorldSync.GetWorldObjects<QSBVesselComputer>().Where(x => x.AttachedObject.GetValue<bool>("_initialized") && x.AttachedObject.GetNumTextBlocks() > 0))
			{
				vesselComputer.GetTranslatedIds().ForEach(id
					=> QSBEventManager.FireEvent(EventNames.QSBTextTranslated, NomaiTextType.VesselComputer, vesselComputer.ObjectId, id));
			}

			QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ForEach(x
				=> QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, x.ObjectId, x.ControllingPlayer));

			QSBWorldSync.GetWorldObjects<QSBCampfire>().ForEach(campfire
				=> QSBEventManager.FireEvent(EventNames.QSBCampfireState, campfire.ObjectId, campfire.GetState()));

			QSBWorldSync.GetWorldObjects<QSBFragment>().ForEach(fragment
				=> QSBEventManager.FireEvent(EventNames.QSBFragmentResync, fragment));

			QSBWorldSync.GetWorldObjects<QSBTornado>().ForEach(tornado
				=> QSBEventManager.FireEvent(EventNames.QSBTornadoFormState, tornado));
		}

		/// <summary>
		/// send info for objects we have authority over
		/// </summary>
		private void SendAuthorityObjectInfo()
		{
			foreach (var qsbOrb in QSBWorldSync.GetWorldObjects<QSBOrb>())
			{
				if (!qsbOrb.TransformSync.enabled ||
				    !qsbOrb.TransformSync.HasAuthority ||
				    qsbOrb.AttachedObject._orbBody.IsSuspended())
				{
					continue;
				}

				QSBEventManager.FireEvent(EventNames.QSBOrbDrag, qsbOrb, qsbOrb.AttachedObject._isBeingDragged);

				for (var slotIndex = 0; slotIndex < qsbOrb.AttachedObject._slots.Length; slotIndex++)
				{
					var slot = qsbOrb.AttachedObject._slots[slotIndex];
					QSBEventManager.FireEvent(EventNames.QSBOrbSlotted, qsbOrb, slotIndex, qsbOrb.AttachedObject._occupiedSlot == slot);
				}
			}
		}
	}
}
