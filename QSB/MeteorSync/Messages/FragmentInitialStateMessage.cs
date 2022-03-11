using OWML.Common;
using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;

namespace QSB.MeteorSync.Messages;

public class FragmentInitialStateMessage : QSBWorldObjectMessage<QSBFragment,
	(float OrigIntegrity, float Integrity, float LeashLength)>
{
	public FragmentInitialStateMessage(QSBFragment qsbFragment) : base((
		qsbFragment.AttachedObject._origIntegrity,
		qsbFragment.AttachedObject._integrity,
		(float)qsbFragment.LeashLength // will have a value at this point, so cast is okay
	)) { }

	public override void OnReceiveRemote()
	{
		WorldObject.AttachedObject._origIntegrity = Data.OrigIntegrity;
		WorldObject.SetIntegrity(Data.Integrity);
		if (WorldObject.LeashLength != null)
		{
			DebugLog.ToConsole($"leash length for {WorldObject} already set", MessageType.Warning);
			return;
		}

		WorldObject.LeashLength = Data.LeashLength;
	}
}
