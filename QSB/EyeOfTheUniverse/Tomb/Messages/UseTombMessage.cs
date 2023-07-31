using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.Tomb.Messages;

public class UseTombMessage : QSBMessage<bool>
{
	public UseTombMessage(bool use) : base(use) { }

	public override void OnReceiveRemote()
	{
		var tomb = QSBWorldSync.GetUnityObject<EyeTombController>();

		if (Data)
		{
			if (!tomb._hasMovedSignalDeeper)
			{
				tomb._hasMovedSignalDeeper = true;
				tomb._buriedSignal.transform.position = tomb._signalDeepSocket.position;
			}
		}

		tomb._interactReceiver.SetInteractionEnabled(!Data);
	}
}
