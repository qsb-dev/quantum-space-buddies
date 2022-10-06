using QSB.Messaging;
using QSB.Player;

namespace QSB.Audio.Messages;

internal class PlayerAudioControllerUpdateHazardDamageMessage : QSBMessage<(uint userID, HazardVolume.HazardType latestHazardType)>
{
	public PlayerAudioControllerUpdateHazardDamageMessage((uint userID, HazardVolume.HazardType latestHazardType) data) : base(data) { }

	public override void OnReceiveRemote() =>
		QSBPlayerManager.GetPlayer(Data.userID)?.AudioController.SetHazardDamage(Data.latestHazardType);
}
