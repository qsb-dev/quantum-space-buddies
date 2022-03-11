using OWML.Common;
using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;

namespace QSB.MeteorSync.Messages;

/// <summary>
/// original integrity, leash length
/// </summary>
public class FragmentInitialStateMessage : QSBWorldObjectMessage<QSBFragment, (float OrigIntegrity, float LeashLength)>
{
	public FragmentInitialStateMessage(float origIntegrity, float leashLength) : base((origIntegrity, leashLength)) { }

	public override void OnReceiveRemote()
	{
		WorldObject.AttachedObject._origIntegrity = Data.OrigIntegrity;
		if (WorldObject.LeashLength == null)
		{
			WorldObject.LeashLength = Data.LeashLength;
		}
		else
		{
			DebugLog.ToConsole($"leash length for {WorldObject} already set", MessageType.Warning);
		}
	}
}
