using QSB.Messaging;
using QSB.Utility.Deterministic;

namespace QSB.WorldSync.Messages;

/// <summary>
/// Sent by clients to the server after receiving a RequestHashBreakdown message.
/// </summary>
public class WorldObjectInfoMessage : QSBMessage<(string fullPath, string managerName)>
{
	public WorldObjectInfoMessage(IWorldObject obj, string managerName) : base((obj.AttachedObject.DeterministicPath(), managerName)) => To = 0;

	public override void OnReceiveRemote()
	{
		HashErrorAnalysis.Instances[Data.managerName].OnReceiveMessage(Data.fullPath);
	}
}
