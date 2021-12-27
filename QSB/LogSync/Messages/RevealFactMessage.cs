using QSB.Messaging;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.LogSync.Messages
{
	public class RevealFactMessage : QSBMessage
	{
		private string FactId;
		private bool SaveGame;
		private bool ShowNotification;

		public RevealFactMessage(string id, bool saveGame, bool showNotification)
		{
			FactId = id;
			SaveGame = saveGame;
			ShowNotification = showNotification;
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(FactId);
			writer.Write(SaveGame);
			writer.Write(ShowNotification);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			FactId = reader.ReadString();
			SaveGame = reader.ReadBoolean();
			ShowNotification = reader.ReadBoolean();
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveLocal()
		{
			if (QSBCore.IsHost)
			{
				QSBWorldSync.AddFactReveal(FactId, SaveGame);
			}
		}

		public override void OnReceiveRemote()
		{
			if (QSBCore.IsHost)
			{
				QSBWorldSync.AddFactReveal(FactId, SaveGame);
			}

			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}

			Locator.GetShipLogManager().RevealFact(FactId, SaveGame, ShowNotification);
		}
	}
}