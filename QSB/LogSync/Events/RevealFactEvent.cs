using QSB.Events;
using QSB.WorldSync;

namespace QSB.LogSync.Events
{
	public class RevealFactEvent : QSBEvent<RevealFactMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<string, bool, bool>.AddListener(EventNames.QSBRevealFact, Handler);
		public override void CloseListener() => GlobalMessenger<string, bool, bool>.RemoveListener(EventNames.QSBRevealFact, Handler);

		private void Handler(string id, bool saveGame, bool showNotification) => SendEvent(CreateMessage(id, saveGame, showNotification));

		private RevealFactMessage CreateMessage(string id, bool saveGame, bool showNotification) => new()
		{
			AboutId = LocalPlayerId,
			FactId = id,
			SaveGame = saveGame,
			ShowNotification = showNotification
		};

		public override void OnReceiveLocal(bool server, RevealFactMessage message)
		{
			if (server)
			{
				QSBWorldSync.AddFactReveal(message.FactId, message.SaveGame, message.ShowNotification);
			}
		}

		public override void OnReceiveRemote(bool server, RevealFactMessage message)
		{
			if (server)
			{
				QSBWorldSync.AddFactReveal(message.FactId, message.SaveGame, message.ShowNotification);
			}

			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}

			Locator.GetShipLogManager().RevealFact(message.FactId, message.SaveGame, message.ShowNotification);
		}
	}
}
