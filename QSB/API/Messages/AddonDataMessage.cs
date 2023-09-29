using QSB.Messaging;
using QSB.Utility;

namespace QSB.API.Messages;

public class AddonDataMessage : QSBMessage<(int hash, byte[] data, bool receiveLocally)>
{
	public AddonDataMessage(int hash, object data, bool receiveLocally) : base((hash, data.ToBytes(), receiveLocally)) { }

	public override void OnReceiveLocal()
	{
		if (Data.receiveLocally)
		{
			OnReceiveRemote();
		}
	}

	public override void OnReceiveRemote()
	{
		var obj = Data.data.ToObject();
		AddonDataManager.OnReceiveDataMessage(Data.hash, obj, From);
	}
}
