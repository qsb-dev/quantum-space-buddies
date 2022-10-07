using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ModelShip.Messages;

internal class CrashModelShipMessage : QSBMessage
{
	public CrashModelShipMessage() { }

	public override void OnReceiveRemote()
	{
		var crashBehaviour = QSBWorldSync.GetUnityObject<ModelShipCrashBehavior>();
		crashBehaviour._crashEffect.Play();
		crashBehaviour.gameObject.GetComponent<OWAudioSource>().PlayOneShot(AudioType.TH_ModelShipCrash);
	}
}
