using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Audio.Messages;


public class PlayerMovementAudioFootstepMessage : QSBMessage<(AudioType audioType, float pitch, uint userID)>
{
	public PlayerMovementAudioFootstepMessage(AudioType audioType, float pitch, uint userID) : base((audioType, pitch, userID)) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote() =>
		QSBPlayerManager.GetPlayer(Data.userID)?.AudioController?.PlayFootstep(Data.audioType, Data.pitch);
}
