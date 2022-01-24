using Mirror;
using QSB.Messaging;
using QSB.Player;
using QSB.TriggerSync.WorldObjects;
using QSB.Utility;
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

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.WriteArray(_playerIds);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			_playerIds = reader.ReadArray<uint>();
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
