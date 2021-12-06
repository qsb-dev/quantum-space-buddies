using OWML.Utils;
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
using System.Linq;

namespace QSB.Player.Events
{
	// Can be sent by any client (including host) to signal they want latest worldobject, player, and server infomation
	public class RequestStateResyncEvent : QSBEvent<PlayerMessage>
	{
		public override bool RequireWorldObjectsReady => false;

		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBRequestStateResync, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBRequestStateResync, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private PlayerMessage CreateMessage() => new()
		{
			AboutId = LocalPlayerId
		};

		public override void OnReceiveRemote(bool isHost, PlayerMessage message)
		{
			// if host, send worldobject and server states
			if (isHost)
			{
				QSBEventManager.FireEvent(EventNames.QSBServerState, ServerStateManager.Instance.GetServerState());
				QSBEventManager.FireEvent(EventNames.QSBPlayerInformation);

				SendWorldObjectInfo();
			}
			// if client, send player and client states
			else
			{
				QSBEventManager.FireEvent(EventNames.QSBPlayerInformation);
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

			var list = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ToList();
			for (var i = 0; i < list.Count; i++)
			{
				QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, i, list[i].ControllingPlayer);
			}

			QSBWorldSync.GetWorldObjects<QSBCampfire>().ForEach(campfire
				=> QSBEventManager.FireEvent(EventNames.QSBCampfireState, campfire.ObjectId, campfire.GetState()));

			QSBWorldSync.GetWorldObjects<QSBFragment>().ForEach(fragment
				=> QSBEventManager.FireEvent(EventNames.QSBFragmentResync, fragment));
		}
	}
}
