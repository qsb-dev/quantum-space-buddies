using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Audio.Messages;


public class PlayerAudioControllerOneShotMessage : QSBMessage<(AudioType audioType, uint userID)>
{
	public PlayerAudioControllerOneShotMessage(AudioType audioType, uint userID) : base((audioType, userID)) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote() =>
		QSBPlayerManager.GetPlayer(Data.userID)?.AudioController?.PlayOneShot(Data.audioType);
}
