using QSB.Messaging;
using QSB.Player;
using QSB.TriggerSync.WorldObjects;
using QSB.Utility;
using QuantumUNET.Transport;
using System.Collections.Generic;
using System.Linq;

namespace QSB.TriggerSync.Messages
{
	/// <summary>
	/// always sent by host
	/// </summary>
	public class TriggerResyncMessage : QSBWorldObjectMessage<IQSBTrigger>
	{
		private uint[] _playerIds;

		public TriggerResyncMessage(IEnumerable<PlayerInfo> occupants) =>
			_playerIds = occupants.Select(x => x.PlayerId).ToArray();

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_playerIds.Length);
			_playerIds.ForEach(writer.Write);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			_playerIds = new uint[reader.ReadInt32()];
			for (var i = 0; i < _playerIds.Length; i++)
			{
				_playerIds[i] = reader.ReadUInt32();
			}
		}

		public override void OnReceiveRemote()
		{
			var serverOccupants = _playerIds.Select(QSBPlayerManager.GetPlayer).ToList();
			foreach (var added in serverOccupants.Except(WorldObject.Occupants))
			{
				WorldObject.Enter(added);
			}

			foreach (var removed in WorldObject.Occupants.Except(serverOccupants))
			{
				WorldObject.Exit(removed);
			}
		}
	}
}
