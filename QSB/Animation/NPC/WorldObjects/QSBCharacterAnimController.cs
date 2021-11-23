using OWML.Utils;
using QSB.Player;
using System.Collections.Generic;

namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBCharacterAnimController : NpcAnimController<CharacterAnimController>
	{
		private readonly List<PlayerInfo> _playersInHeadZone = new();

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

		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._dialogueTree;

		public override bool InConversation()
			=> AttachedObject._inConversation;
	}
}
