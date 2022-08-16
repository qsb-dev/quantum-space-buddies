using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.Tomb.Messages;

internal class CloseDoorMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		var mirror = QSBWorldSync.GetUnityObject<EyeMirrorController>();
		mirror._door.Close();
	}
}
