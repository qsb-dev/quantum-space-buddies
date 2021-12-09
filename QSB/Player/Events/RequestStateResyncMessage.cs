﻿using System.Linq;
using OWML.Utils;
using QSB.CampfireSync.Events;
using QSB.CampfireSync.WorldObjects;
using QSB.ClientServerStateSync;
using QSB.Events;
using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QSB.QuantumSync;
using QSB.Tools.TranslatorTool.TranslationSync;
using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.Player.Events
{
	public class RequestStateResyncMessage : QSBMessage
	{
		public override void Serialize(QNetworkWriter writer) { }
		public override void Deserialize(QNetworkReader reader) { }

		public override void OnReceiveRemote(uint from)
		{
			// send response only to the requesting client
			QSBEventManager.ForIdOverride = from;
			try
			{
				// if host, send worldobject and server states
				if (QSBCore.IsHost)
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
			}
			finally
			{
				QSBEventManager.ForIdOverride = uint.MaxValue;
			}
		}

		private static void SendWorldObjectInfo()
		{
			QSBWorldSync.DialogueConditions.ForEach(condition
				=> QSBEventManager.FireEvent(EventNames.DialogueConditionChanged, condition.Key, condition.Value));

			QSBWorldSync.ShipLogFacts.ForEach(fact
				=> QSBEventManager.FireEvent(EventNames.QSBRevealFact, fact.Id, fact.SaveGame, false));

			foreach (var wallText in QSBWorldSync.GetWorldObjects<QSBWallText>().Where(x => x.AttachedObject._initialized && x.AttachedObject.GetNumTextBlocks() > 0))
			{
				wallText.GetTranslatedIds().ForEach(id
					=> QSBEventManager.FireEvent(EventNames.QSBTextTranslated, NomaiTextType.WallText, wallText.ObjectId, id));
			}

			foreach (var computer in QSBWorldSync.GetWorldObjects<QSBComputer>().Where(x => x.AttachedObject._initialized && x.AttachedObject.GetNumTextBlocks() > 0))
			{
				computer.GetTranslatedIds().ForEach(id
					=> QSBEventManager.FireEvent(EventNames.QSBTextTranslated, NomaiTextType.Computer, computer.ObjectId, id));
			}

			foreach (var vesselComputer in QSBWorldSync.GetWorldObjects<QSBVesselComputer>().Where(x => x.AttachedObject._initialized && x.AttachedObject.GetNumTextBlocks() > 0))
			{
				vesselComputer.GetTranslatedIds().ForEach(id
					=> QSBEventManager.FireEvent(EventNames.QSBTextTranslated, NomaiTextType.VesselComputer, vesselComputer.ObjectId, id));
			}

			var list = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ToList();
			for (var i = 0; i < list.Count; i++)
			{
				QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, i, list[i].ControllingPlayer);
			}

			QSBWorldSync.GetWorldObjects<QSBCampfire>().ForEach(campfire
				=> campfire.SendMessage(new CampfireStateMessage
				{
					Value = campfire.GetState()
				}));

			QSBWorldSync.GetWorldObjects<QSBFragment>().ForEach(fragment
				=> QSBEventManager.FireEvent(EventNames.QSBFragmentResync, fragment));
		}
	}
}
