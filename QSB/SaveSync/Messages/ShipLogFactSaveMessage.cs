using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.SaveSync.Messages
{
	internal class ShipLogFactSaveMessage : QSBMessage
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

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_id);
			writer.Write(_revealOrder);
			writer.Write(_read);
			writer.Write(_newlyRevealed);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			_id = reader.ReadString();
			_revealOrder = reader.ReadInt32();
			_read = reader.ReadBoolean();
			_newlyRevealed = reader.ReadBoolean();
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
}
