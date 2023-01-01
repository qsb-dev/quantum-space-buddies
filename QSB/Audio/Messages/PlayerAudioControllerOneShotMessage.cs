using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Audio.Messages;

public class PlayerAudioControllerOneShotMessage : QSBMessage<(AudioType audioType, uint userID, float pitch, float volume)>
{
	public PlayerAudioControllerOneShotMessage(AudioType audioType, uint userID, float pitch = 1f, float volume = 1f) : base((audioType, userID, pitch, volume)) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote() =>
		QSBPlayerManager.GetPlayer(Data.userID)?.AudioController?.PlayOneShot(Data.audioType, Data.pitch, Data.volume);
}
