using OWML.Utils;
using QSB.CampfireSync.WorldObjects;
using QSB.Events;
using QSB.Messaging;
using QSB.QuantumSync;
using QSB.TranslationSync;
using QSB.TranslationSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;

namespace QSB.Player.Events
{
	public class PlayerStatesRequestEvent : QSBEvent<PlayerMessage>
	{
		public override EventType Type => EventType.PlayerStatesRequest;

		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBPlayerStatesRequest, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBPlayerStatesRequest, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private PlayerMessage CreateMessage() => new PlayerMessage
		{
			AboutId = LocalPlayerId,
			OnlySendToServer = true
		};

		public override void OnReceiveRemote(bool server, PlayerMessage message)
		{
			DebugLog.DebugWrite($"Get state request from {message.FromId} - isServer?{server}");
			QSBEventManager.FireEvent(EventNames.QSBServerSendPlayerStates);
			foreach (var item in QSBPlayerManager.GetSyncObjects<TransformSync.TransformSync>()
				.Where(x => x != null && x.IsReady && x.ReferenceSector != null))
			{
				QSBEventManager.FireEvent(EventNames.QSBSectorChange, item.NetId.Value, item.ReferenceSector);
			}

			if (!server)
			{
				return;
			}

			// TODO : CLEAN. THIS. SHIT.

			foreach (var condition in QSBWorldSync.DialogueConditions)
			{
				QSBEventManager.FireEvent(EventNames.DialogueCondition, condition.Key, condition.Value);
			}

			foreach (var fact in QSBWorldSync.ShipLogFacts)
			{
				QSBEventManager.FireEvent(EventNames.QSBRevealFact, fact.Id, fact.SaveGame, false);
			}

			foreach (var wallText in QSBWorldSync.GetWorldObjects<QSBWallText>().Where(x => x.AttachedObject.GetValue<bool>("_initialized") && x.AttachedObject.GetNumTextBlocks() > 0))
			{
				foreach (var id in wallText.GetTranslatedIds())
				{
					QSBEventManager.FireEvent(EventNames.QSBTextTranslated, NomaiTextType.WallText, wallText.ObjectId, id);
				}
			}

			foreach (var computer in QSBWorldSync.GetWorldObjects<QSBComputer>().Where(x => x.AttachedObject.GetValue<bool>("_initialized") && x.AttachedObject.GetNumTextBlocks() > 0))
			{
				foreach (var id in computer.GetTranslatedIds())
				{
					QSBEventManager.FireEvent(EventNames.QSBTextTranslated, NomaiTextType.Computer, computer.ObjectId, id);
				}
			}

			foreach (var vesselComputer in QSBWorldSync.GetWorldObjects<QSBVesselComputer>().Where(x => x.AttachedObject.GetValue<bool>("_initialized") && x.AttachedObject.GetNumTextBlocks() > 0))
			{
				foreach (var id in vesselComputer.GetTranslatedIds())
				{
					QSBEventManager.FireEvent(EventNames.QSBTextTranslated, NomaiTextType.VesselComputer, vesselComputer.ObjectId, id);
				}
			}

			var list = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ToList();
			for (var i = 0; i < list.Count; i++)
			{
				QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, i, list[i].ControllingPlayer);
			}

			foreach (var campfire in QSBWorldSync.GetWorldObjects<QSBCampfire>())
			{
				QSBEventManager.FireEvent(EventNames.QSBCampfireState, campfire.ObjectId, campfire.GetState());
			}
		}
	}
}