using QSB.Messaging;
using QSB.ShipSync;
using QSB.WorldSync;

namespace QSB.Audio.Messages;

public class ShipThrusterAudioOneShotMessage : QSBMessage<(AudioType audioType, float pitch, float volume)>
{
	public ShipThrusterAudioOneShotMessage(AudioType audioType, float pitch = 1f, float volume = 1f) : base((audioType, pitch, volume)) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var source = ShipManager.Instance.ShipThrusterAudio._rotationalSource;
		source.pitch = Data.pitch;
		source.PlayOneShot(Data.audioType, Data.volume);
	}
}
