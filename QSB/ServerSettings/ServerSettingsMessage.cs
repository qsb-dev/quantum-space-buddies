using Mirror;
using QSB.Messaging;

namespace QSB.ServerSettings;

internal class ServerSettingsMessage : QSBMessage
{
	private bool _showPlayerNames;

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(QSBCore.ShowPlayerNames);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		_showPlayerNames = reader.ReadBool();
	}

	public override void OnReceiveRemote()
	{
		ServerSettingsManager.ShowPlayerNames = _showPlayerNames;
	}
}
