using OWML.Common;
using QSB.CampfireSync.Messages;
using QSB.CampfireSync.WorldObjects;
using QSB.ClientServerStateSync;
using QSB.ClientServerStateSync.Messages;
using QSB.ConversationSync.Messages;
using QSB.LogSync.Messages;
using QSB.Messaging;
using QSB.MeteorSync.Messages;
using QSB.MeteorSync.WorldObjects;
using QSB.OrbSync.Messages;
using QSB.OrbSync.WorldObjects;
using QSB.QuantumSync.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.Tools.TranslatorTool.TranslationSync.Messages;
using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;
using QSB.TornadoSync.Messages;
using QSB.TornadoSync.WorldObjects;
using QSB.TriggerSync.Messages;
using QSB.TriggerSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Player.Messages
{
	// Can be sent by any client (including host) to signal they want latest worldobject, player, and server information
	public class RequestStateResyncMessage : QSBMessage
	{
		/// <summary>
		/// set to true when we send this, and false when we receive a player info message back. <br/>
		/// this prevents message spam a bit.
		/// </summary>
		internal static bool _waitingForEvent;

		/// <summary>
		/// used instead of QSBMessageManager.Send to do the extra check
		/// </summary>
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
					DebugLog.ToConsole($"Did not receive PlayerInformationEvent in time. Setting _waitingForEvent to false.", MessageType.Info);
					_waitingForEvent = false;
				}
			}, 60);
		}

		public override void OnReceiveRemote()
		{
			// if host, send worldobject and server states
			if (QSBCore.IsHost)
			{
				new ServerStateMessage(ServerStateManager.Instance.GetServerState()) { To = From }.Send();
				new PlayerInformationMessage { To = From }.Send();

				if (QSBWorldSync.AllObjectsReady)
				{
					SendWorldObjectInfo();
				}
			}
			// if client, send player and client states
			else
			{
				new PlayerInformationMessage { To = From }.Send();
			}

			if (QSBWorldSync.AllObjectsReady)
			{
				SendAuthorityObjectInfo();
			}
		}

		private void SendWorldObjectInfo()
		{
			QSBWorldSync.DialogueConditions.ForEach(condition
				=> new DialogueConditionMessage(condition.Key, condition.Value) { To = From }.Send());

			QSBWorldSync.ShipLogFacts.ForEach(fact
				=> new RevealFactMessage(fact.Id, fact.SaveGame, false) { To = From }.Send());

			foreach (var text in QSBWorldSync.GetWorldObjects<QSBNomaiText>())
			{
				text.GetTranslatedIds().ForEach(id =>
					text.SendMessage(new SetAsTranslatedMessage(id) { To = From }));
			}

			QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ForEach(x =>
			{
				x.SendMessage(new QuantumAuthorityMessage(x.ControllingPlayer) { To = From });

				if (x is QSBQuantumMoon qsbQuantumMoon)
				{
					var moon = qsbQuantumMoon.AttachedObject;
					var moonBody = moon._moonBody;
					var stateIndex = moon.GetStateIndex();
					var orbit = moon._orbits.First(y => y.GetStateIndex() == stateIndex);
					var orbitBody = orbit.GetAttachedOWRigidbody();
					var relPos = moonBody.GetWorldCenterOfMass() - orbitBody.GetWorldCenterOfMass();
					var relVel = moonBody.GetVelocity() - orbitBody.GetVelocity();
					var onUnitSphere = relPos.normalized;
					var perpendicular = Vector3.Cross(relPos, Vector3.up).normalized;
					var orbitAngle = (int)OWMath.WrapAngle(OWMath.Angle(perpendicular, relVel, relPos));

					new MoonStateChangeMessage(stateIndex, onUnitSphere, orbitAngle) { To = From }.Send();
				}
			});

			QSBWorldSync.GetWorldObjects<QSBCampfire>().ForEach(campfire
				=> campfire.SendMessage(new CampfireStateMessage(campfire.GetState()) { To = From }));

			QSBWorldSync.GetWorldObjects<QSBFragment>().ForEach(fragment
				=> fragment.SendMessage(new FragmentResyncMessage(fragment) { To = From }));

			QSBWorldSync.GetWorldObjects<QSBTornado>().ForEach(tornado
				=> tornado.SendMessage(new TornadoFormStateMessage(tornado.FormState) { To = From }));

			QSBWorldSync.GetWorldObjects<IQSBTrigger>().ForEach(trigger
				=> trigger.SendMessage(new TriggerResyncMessage(trigger.Occupants) { To = From }));
		}

		/// <summary>
		/// send info for objects we have authority over
		/// </summary>
		private void SendAuthorityObjectInfo()
		{
			foreach (var qsbOrb in QSBWorldSync.GetWorldObjects<QSBOrb>())
			{
				if (!qsbOrb.TransformSync.hasAuthority)
				{
					continue;
				}

				qsbOrb.SendMessage(new OrbDragMessage(qsbOrb.AttachedObject._isBeingDragged) { To = From });
				qsbOrb.SendMessage(new OrbSlotMessage(qsbOrb.AttachedObject._slots.IndexOf(qsbOrb.AttachedObject._occupiedSlot)) { To = From });
			}
		}
	}
}
