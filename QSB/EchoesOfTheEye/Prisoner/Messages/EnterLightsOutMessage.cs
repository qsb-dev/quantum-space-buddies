using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

internal class EnterLightsOutMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		var director = QSBWorldSync.GetUnityObject<PrisonerDirector>();
		director._lightsOutTrigger.OnEntry -= director.OnEnterLightsOutTrigger;
		director._prisonLighting.FadeTo(0, 1);
		director._hangingLampSource.PlayOneShot(AudioType.Candle_Extinguish, 1f);
		director._lightsOnAudioVolume.SetVolumeActivation(false);
	}
}
