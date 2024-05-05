using OWML.Common;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.WorldSync.Messages;

/// <summary>
/// Sent to clients from the server when a client has an incorrect WorldObject hash.
/// </summary>
internal class RequestHashBreakdownMessage : QSBMessage<string>
{
	public RequestHashBreakdownMessage(string managerName) : base(managerName) { }

	public override void OnReceiveRemote()
	{
		DebugLog.ToConsole($"Received RequestHashBreakdownMessage for {Data}", MessageType.Error);
		var objects = QSBWorldSync.GetWorldObjectsFromManager(Data);

		foreach (var worldObject in objects)
		{
			new WorldObjectInfoMessage(worldObject, Data).Send();
		}

		DebugLog.ToConsole("- Sending finished message.", MessageType.Error);
		new DataDumpFinishedMessage(Data).Send();
	}
}
