using QSB.Player;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.Animation.NPC.WorldObjects
{
	class QSBSolanumAnimController : WorldObject<SolanumAnimController>
	{
		private readonly List<PlayerInfo> _playersInHeadZone = new List<PlayerInfo>();

		public override void Init(SolanumAnimController controller, int id)
		{
			ObjectId = id;
			AttachedObject = controller;
		}

		public List<PlayerInfo> GetPlayersInHeadZone()
			=> _playersInHeadZone;

		public void AddPlayerToHeadZone(PlayerInfo player)
		{
			if (_playersInHeadZone.Contains(player))
			{
				return;
			}

			_playersInHeadZone.Add(player);
		}

		public void RemovePlayerFromHeadZone(PlayerInfo player)
		{
			if (!_playersInHeadZone.Contains(player))
			{
				return;
			}

			_playersInHeadZone.Remove(player);
		}
	}
}
