using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using QSB.Messaging;

namespace QSB.API.Messages;

public class AddonDataMessage : QSBMessage<(string messageType, byte[] data, bool receiveLocally)>
{
	public AddonDataMessage(string messageType, object data, bool receiveLocally) : base((messageType, Obj2Bytes(data), receiveLocally)) { }

	private static byte[] Obj2Bytes(object obj)
	{
		using var ms = new MemoryStream();
		var bf = new BinaryFormatter();
		bf.Serialize(ms, obj);
		var bytes = ms.ToArray();
		return bytes;
	}

	private static object Bytes2Obj(byte[] bytes)
	{
		using var ms = new MemoryStream(bytes);
		var bf = new BinaryFormatter();
		var obj = bf.Deserialize(ms);
		return obj;
	}

	public override void OnReceiveLocal()
	{
		if (Data.receiveLocally)
		{
			OnReceiveRemote();
		}
	}

	public override void OnReceiveRemote()
	{
		var obj = Bytes2Obj(Data.data);
		AddonDataManager.OnReceiveDataMessage(Data.messageType, obj, From);
	}
}
