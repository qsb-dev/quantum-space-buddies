using System.Linq;
using OWML.Utils;
using QSB.CampfireSync.WorldObjects;
using QSB.ClientServerStateSync;
using QSB.ConversationSync.Messages;
using QSB.Events;
using QSB.Messaging;
using QSB.MeteorSync.Messages;
using QSB.MeteorSync.WorldObjects;
using QSB.OrbSync.Messages;
using QSB.OrbSync.WorldObjects;
using QSB.QuantumSync.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.Tools.TranslatorTool.TranslationSync;
using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;
using QSB.TornadoSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Player.Messages
{
	// Can be sent by any client (including host) to signal they want latest worldobject, player, and server infomation
	public class RequestStateResyncMessage : QSBMessage
	{
		/// <summary>
		/// set to true when we send this, and false when we receive a player info message back. <br/>
		/// this prevents message spam a bit.
		/// </summary>
		internal static bool _waitingForEvent;

		public void Send()
		{
			if (_waitingForEvent)
			{
				return;
			}

			_waitingForEvent = true;
			QSBMessageManager.Send(this);
		}

		public override void OnReceiveLocal()
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

		public override void OnReceiveRemote()
		{
			// send response only to the requesting client
			QSBEventManager.ForIdOverride = From;
			try
			{
				// if host, send worldobject and server states
				if (QSBCore.IsHost)
				{
					ServerStateManager.Instance.SendChangeServerStateMessage(ServerStateManager.Instance.GetServerState());
					new PlayerInformationMessage().Send();

					if (WorldObjectManager.AllObjectsReady)
					{
						SendWorldObjectInfo();
					}
				}
				// if client, send player and client states
				else
				{
					new PlayerInformationMessage().Send();
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

		private static void SendWorldObjectInfo()
		{
			QSBWorldSync.DialogueConditions.ForEach(condition
				=> new DialogueConditionMessage(condition.Key, condition.Value).Send());

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

			QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ForEach(x =>
			{
				x.SendMessage(new QuantumAuthorityMessage(x.ControllingPlayer));

				if (x is QSBQuantumMoon qsbQuantumMoon)
				{
					int stateIndex;
					Vector3 onUnitSphere;
					int orbitAngle;

					var moon = qsbQuantumMoon.AttachedObject;
					var moonBody = moon._moonBody;
					stateIndex = moon.GetStateIndex();
					var orbit = moon._orbits.First(y => y.GetStateIndex() == stateIndex);
					var orbitBody = orbit.GetAttachedOWRigidbody();
					var relPos = moonBody.GetWorldCenterOfMass() - orbitBody.GetWorldCenterOfMass();
					var relVel = moonBody.GetVelocity() - orbitBody.GetVelocity();
					onUnitSphere = relPos.normalized;
					var perpendicular = Vector3.Cross(relPos, Vector3.up).normalized;
					orbitAngle = (int)OWMath.WrapAngle(OWMath.Angle(perpendicular, relVel, relPos));

					QSBEventManager.FireEvent(EventNames.QSBMoonStateChange, stateIndex, onUnitSphere, orbitAngle);
				}
			});

			QSBWorldSync.GetWorldObjects<QSBCampfire>().ForEach(campfire
				=> QSBEventManager.FireEvent(EventNames.QSBCampfireState, campfire.ObjectId, campfire.GetState()));

			QSBWorldSync.GetWorldObjects<QSBFragment>().ForEach(fragment
				=> fragment.SendMessage(new FragmentResyncMessage(fragment)));

			QSBWorldSync.GetWorldObjects<QSBTornado>().ForEach(tornado
				=> QSBEventManager.FireEvent(EventNames.QSBTornadoFormState, tornado));
		}

		/// <summary>
		/// send info for objects we have authority over
		/// </summary>
		private static void SendAuthorityObjectInfo()
		{
			foreach (var qsbOrb in QSBWorldSync.GetWorldObjects<QSBOrb>())
			{
				if (!qsbOrb.TransformSync.enabled ||
				    !qsbOrb.TransformSync.HasAuthority)
				{
					continue;
				}

				qsbOrb.SendMessage(new OrbDragMessage(qsbOrb.AttachedObject._isBeingDragged));
				qsbOrb.SendMessage(new OrbSlotMessage(qsbOrb.AttachedObject._slots.IndexOf(qsbOrb.AttachedObject._occupiedSlot)));
			}
		}
	}
}