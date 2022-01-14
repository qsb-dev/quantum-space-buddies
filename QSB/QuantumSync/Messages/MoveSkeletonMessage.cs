using Mirror;
using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;

namespace QSB.QuantumSync.Messages
{
	internal class MoveSkeletonMessage : QSBWorldObjectMessage<QSBQuantumSkeletonTower>
	{
		private int _index;

		public MoveSkeletonMessage(int index) => _index = index;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_index);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			_index = reader.ReadInt();
		}

		public override void OnReceiveRemote() => WorldObject.MoveSkeleton(_index);
	}
}
