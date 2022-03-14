using Mirror;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Utility.LinkedWorldObject;

/// <summary>
/// sent to the host when all world objects are added.
/// used in order to link world objects to network behaviours.
/// </summary>
public class RequestLinksMessage : QSBMessage
{
	public RequestLinksMessage() => To = 0;

	public override void OnReceiveRemote() =>
		Delay.RunWhen(() => QSBWorldSync.AllObjectsAdded,
			() => SendLinks(From));

	private static void SendLinks(uint to)
	{
		DebugLog.DebugWrite($"sending world object links to {to}");

		foreach (var worldObject in QSBWorldSync.GetWorldObjects<ILinkedWorldObject<NetworkBehaviour>>())
		{
			new LinkMessage(worldObject, worldObject.NetworkBehaviour) { To = to }.Send();
		}
	}
}
