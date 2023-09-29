using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class PlayVoiceAudioMessage : QSBWorldObjectMessage<QSBGhostEffects, (AudioType audioType, float volumeScale, bool near)>
{
	public PlayVoiceAudioMessage(AudioType audioType, float volumeScale, bool near) : base((audioType, volumeScale, near)) { }

	public override void OnReceiveRemote()
	{
		if (Data.near)
		{
			WorldObject.PlayVoiceAudioNear(Data.audioType, Data.volumeScale, true);
			return;
		}

		WorldObject.PlayVoiceAudioFar(Data.audioType, Data.volumeScale, true);
	}
}
