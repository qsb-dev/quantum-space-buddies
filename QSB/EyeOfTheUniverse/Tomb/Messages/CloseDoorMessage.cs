using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.Tomb.Messages;

public class CloseDoorMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		var mirror = QSBWorldSync.GetUnityObject<EyeMirrorController>();
		mirror._door.Close();
	}
}
