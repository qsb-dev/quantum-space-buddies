using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using QSB.Messaging;

namespace QSB.API.Messages;
public class AddonDataMessage : QSBMessage<(string messageType, byte[] data)>
{
	public AddonDataMessage(string messageType, object data) : base((messageType, ObjectToByteArray(data))) {}

	private static byte[] ObjectToByteArray(object obj)
	{
		var bf = new BinaryFormatter();
		using var ms = new MemoryStream();
		bf.Serialize(ms, obj);
		return ms.ToArray();
	}

	private static object ByteArrayToObject(byte[] arrBytes)
	{
		using var memStream = new MemoryStream();
		var binForm = new BinaryFormatter();
		memStream.Write(arrBytes, 0, arrBytes.Length);
		memStream.Seek(0, SeekOrigin.Begin);
		var obj = binForm.Deserialize(memStream);
		return obj;
	}

	public override void OnReceiveRemote()
	{
		var obj = ByteArrayToObject(Data.data);
		AddonDataManager.OnReceiveDataMessage(Data.messageType, obj);
	}
}
