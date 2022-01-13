using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QuantumUNET.Transport;
using System.Collections.Generic;
using System.Linq;

namespace QSB.TriggerSync
{
	/// <summary>
	/// always sent by host
	/// </summary>
	public class TriggerResyncMessage : QSBWorldObjectMessage<QSBTrigger>
	{
		private uint[] _playerIds;

		public TriggerResyncMessage(IEnumerable<PlayerInfo> players) =>
			_playerIds = players.Select(x => x.PlayerId).ToArray();

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
			var serverPlayers = _playerIds.Select(QSBPlayerManager.GetPlayer).ToList();
			foreach (var added in serverPlayers.Except(WorldObject.Players))
			{
				WorldObject.Enter(added);
			}

			foreach (var removed in WorldObject.Players.Except(serverPlayers))
			{
				WorldObject.Exit(removed);
			}
		}
	}
}
