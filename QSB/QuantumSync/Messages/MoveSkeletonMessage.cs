using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;

namespace QSB.QuantumSync.Messages
{
	internal class MoveSkeletonMessage : QSBWorldObjectMessage<QSBQuantumSkeletonTower, int>
	{
		public MoveSkeletonMessage(int index) => Value = index;

		public override void OnReceiveRemote() => WorldObject.MoveSkeleton(Value);
	}
}
