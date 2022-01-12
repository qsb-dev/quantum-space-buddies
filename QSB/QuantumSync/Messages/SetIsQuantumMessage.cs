using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;

namespace QSB.QuantumSync.Messages
{
	public class SetIsQuantumMessage : QSBBoolWorldObjectMessage<IQSBQuantumObject>
	{
		public SetIsQuantumMessage(bool isQuantum) => Value = isQuantum;

		public override void OnReceiveRemote() => ((QuantumObject)WorldObject.ReturnObject())._isQuantum = Value;
	}
}
