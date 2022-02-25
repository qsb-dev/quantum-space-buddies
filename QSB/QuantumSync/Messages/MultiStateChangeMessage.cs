using OWML.Common;
using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;

namespace QSB.QuantumSync.Messages;

internal class MultiStateChangeMessage : QSBWorldObjectMessage<QSBMultiStateQuantumObject, int>
{
	public MultiStateChangeMessage(int stateIndex) => Value = stateIndex;

	public override void OnReceiveRemote()
	{
		if (WorldObject.ControllingPlayer != From)
		{
			DebugLog.ToConsole($"Error - Got MultiStateChangeEvent for {WorldObject.Name} from {From}, but it's currently controlled by {WorldObject.ControllingPlayer}!", MessageType.Error);
			return;
		}

		WorldObject.ChangeState(Value);
	}
}