using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;

namespace QSB.QuantumSync.Messages
{
	public class SetIsQuantumMessage : QSBWorldObjectMessage<IQSBQuantumObject, bool>
	{
		public SetIsQuantumMessage(bool isQuantum) => Data = isQuantum;

		public override void OnReceiveRemote() => WorldObject.SetIsQuantum(Data);
	}
}