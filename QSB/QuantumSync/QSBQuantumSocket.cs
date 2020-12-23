using OWML.Utils;
using QSB.WorldSync;

namespace QSB.QuantumSync
{
	internal class QSBQuantumSocket : WorldObject<QuantumSocket>
	{
		public QuantumSocket AttachedSocket { get; private set; }

		public override void Init(QuantumSocket quantumSocket, int id)
		{
			ObjectId = id;
			AttachedSocket = quantumSocket;
			AttachedSocket.GetType().SetValue("_randomYRotation", false);
		}
	}
}