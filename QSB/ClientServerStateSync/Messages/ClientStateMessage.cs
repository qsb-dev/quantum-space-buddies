using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.ClientServerStateSync.Messages;

/// <summary>
/// sets the state both locally and remotely
/// </summary>
public class ClientStateMessage : QSBMessage<ClientState>
{
	public ClientStateMessage(ClientState state) : base(state) { }

	public override void OnReceiveLocal()
		=> ClientStateManager.Instance.ChangeClientState(Data);

	public override void OnReceiveRemote()
	{
		if (From == uint.MaxValue)
		{
			DebugLog.ToConsole($"Error - ID is uint.MaxValue!", MessageType.Error);
			return;
		}

		var player = QSBPlayerManager.GetPlayer(From);
		player.State = Data;
	}
}