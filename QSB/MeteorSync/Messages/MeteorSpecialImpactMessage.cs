using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;

namespace QSB.MeteorSync.Messages;

/// <summary>
/// for syncing impact with a remote player/probe
/// </summary>
public class MeteorSpecialImpactMessage : QSBWorldObjectMessage<QSBMeteor>
{
	public override void OnReceiveRemote() => WorldObject.SpecialImpact();
}