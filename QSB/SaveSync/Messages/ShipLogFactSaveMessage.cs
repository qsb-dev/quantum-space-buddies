using Mirror;
using QSB.Messaging;

namespace QSB.SaveSync.Messages;

public class ShipLogFactSaveMessage : QSBMessage
{
	private string _id;
	private int _revealOrder;
	private bool _read;
	private bool _newlyRevealed;

	public ShipLogFactSaveMessage(ShipLogFactSave save)
	{
		_id = save.id;
		_revealOrder = save.revealOrder;
		_read = save.read;
		_newlyRevealed = save.newlyRevealed;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(_id);
		writer.Write(_revealOrder);
		writer.Write(_read);
		writer.Write(_newlyRevealed);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		_id = reader.ReadString();
		_revealOrder = reader.ReadInt();
		_read = reader.ReadBool();
		_newlyRevealed = reader.ReadBool();
	}

	public override void OnReceiveRemote()
	{
		var save = PlayerData.GetShipLogFactSave(_id);

		if (save == null)
		{
			save = new ShipLogFactSave(_id);
			PlayerData.AddShipLogFactSave(save);
		}

		save.revealOrder = _revealOrder;
		save.read = _read;
		save.newlyRevealed = _newlyRevealed;
	}
}