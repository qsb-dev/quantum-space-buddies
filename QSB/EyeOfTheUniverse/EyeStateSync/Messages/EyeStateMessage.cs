using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.EyeStateSync.Messages;

/// <summary>
/// todo SendInitialState
/// </summary>
public class EyeStateMessage : QSBMessage<EyeState>
{
	static EyeStateMessage() => GlobalMessenger<EyeState>.AddListener(OWEvents.EyeStateChanged, Handler);

	private static void Handler(EyeState state)
	{
		if (PlayerTransformSync.LocalInstance)
		{
			new EyeStateMessage(state).Send();
		}
	}

	private EyeStateMessage(EyeState state) : base(state) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveLocal() => QSBPlayerManager.LocalPlayer.EyeState = Data;

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.EyeState = Data;
	}
}