using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.API.Messages;

public class AddonDataMessage : QSBMessage<(string messageType, byte[] data, bool receiveLocally)>
{
	public AddonDataMessage(string messageType, object data, bool receiveLocally) : base((messageType, data.ToBytes(), receiveLocally)) { }

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
		AddonDataManager.OnReceiveDataMessage(Data.messageType, obj, From);
	}
}
