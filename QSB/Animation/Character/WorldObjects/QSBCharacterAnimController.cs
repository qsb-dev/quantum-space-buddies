using OWML.Common;
using OWML.Utils;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.Animation.Character.WorldObjects
{
	class QSBCharacterAnimController : WorldObject<CharacterAnimController>
	{
		private readonly List<PlayerInfo> _playersInHeadZone = new List<PlayerInfo>();

		public override void Init(CharacterAnimController controller, int id)
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

		public void StartConversation()
		{
			AttachedObject.SetValue("_inConversation", true);
			QSBWorldSync.RaiseEvent(AttachedObject, "OnStartConversation");
		}

		public void EndConversation()
		{
			AttachedObject.SetValue("_inConversation", false);
			QSBWorldSync.RaiseEvent(AttachedObject, "OnEndConversation");
		}
	}
}
