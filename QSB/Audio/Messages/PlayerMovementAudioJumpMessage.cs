using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Audio.Messages;

public class PlayerMovementAudioJumpMessage : QSBMessage<(float pitch, uint userID)>
{
	public PlayerMovementAudioJumpMessage(float pitch, uint userID) : base((pitch, userID)) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote() =>
		QSBPlayerManager.GetPlayer(Data.userID)?.AudioController?.OnJump(Data.pitch);
}
