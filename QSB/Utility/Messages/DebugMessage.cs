using QSB.Messaging;

namespace QSB.Utility.Messages
{
	public class DebugMessage : QSBEnumMessage<DebugMessageEnum>
	{
		public DebugMessage(DebugMessageEnum type) => Value = type;

		public override void OnReceiveLocal() => OnReceiveRemote();

		public override void OnReceiveRemote()
		{
			switch (Value)
			{
				case DebugMessageEnum.TriggerSupernova:
					TimeLoop.SetSecondsRemaining(0f);
					break;
			}
		}
	}
}
