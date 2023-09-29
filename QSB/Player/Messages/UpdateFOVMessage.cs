using QSB.Messaging;
using QSB.Utility;

namespace QSB.Player.Messages;

public class UpdateFOVMessage : QSBMessage<float>
{
	static UpdateFOVMessage()
		=> GlobalMessenger<GraphicSettings>.AddListener(
			"GraphicSettingsUpdated",
			(GraphicSettings settings) => new UpdateFOVMessage(settings.fieldOfView).Send());

	private UpdateFOVMessage(float fov) : base(fov) { }

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.Camera.fieldOfView = Data;
	}
}
