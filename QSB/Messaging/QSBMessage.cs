using Mirror;

namespace QSB.Messaging;

public abstract class QSBMessage
{
	/// <summary>
	/// set automatically by Send
	/// </summary>
	// public so it can be accessed by a patch
	public uint From;
	/// <summary>
	/// (default) uint.MaxValue = send to everyone <br/>
	/// 0 = send to host
	/// </summary>
	public uint To = uint.MaxValue;

	/// <summary>
	/// call the base method when overriding
	/// </summary>
	public virtual void Serialize(NetworkWriter writer)
	{
		writer.Write(From);
		writer.Write(To);
	}

	/// <summary>
	/// call the base method when overriding
	/// <para/>
	/// note: no constructor is called before this,
	/// so fields won't be initialized.
	/// </summary>
	public virtual void Deserialize(NetworkReader reader)
	{
		From = reader.Read<uint>();
		To = reader.Read<uint>();
	}

	/// <summary>
	/// checked before calling either OnReceive
	/// </summary>
	public virtual bool ShouldReceive => true;
	public virtual void OnReceiveLocal() { }
	public virtual void OnReceiveRemote() { }
}

public abstract class QSBMessage<D> : QSBMessage
{
	// public so it can be accessed by a patch
	public D Data { get; private set; }
	protected QSBMessage(D data) => Data = data;

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(Data);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		Data = reader.Read<D>();
	}
}
