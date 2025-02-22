using Mirror;
using QSB.Messaging;

namespace QSB.ServerSettings;

public class ServerSettingsMessage : QSBMessage
{
	private bool _showPlayerNames;
	private bool _alwaysShowPlanetIcons;

	public ServerSettingsMessage()
	{
		_showPlayerNames = QSBCore.ShowPlayerNames;
		_alwaysShowPlanetIcons = QSBCore.AlwaysShowPlanetIcons;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(_showPlayerNames);
		writer.Write(_alwaysShowPlanetIcons);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		_showPlayerNames = reader.ReadBool();
		_alwaysShowPlanetIcons = reader.ReadBool();
	}

	public override void OnReceiveRemote()
	{
		ServerSettingsManager.ServerShowPlayerNames = _showPlayerNames;
		ServerSettingsManager.ServerAlwaysShowPlanetIcons = _alwaysShowPlanetIcons;
	}
}
