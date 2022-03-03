using Mirror;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.LogSync.Messages;

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

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(FactId);
		writer.Write(SaveGame);
		writer.Write(ShowNotification);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		FactId = reader.ReadString();
		SaveGame = reader.Read<bool>();
		ShowNotification = reader.Read<bool>();
	}

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

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

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		Locator.GetShipLogManager().RevealFact(FactId, SaveGame, ShowNotification);
	}
}