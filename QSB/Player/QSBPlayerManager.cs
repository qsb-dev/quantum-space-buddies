using OWML.Common;
using QSB.Player.Events;
using QSB.TransformSync;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Player
{
	public static class QSBPlayerManager
	{
		public static uint LocalPlayerId => PlayerTransformSync.LocalInstance.NetIdentity?.NetId.Value ?? uint.MaxValue;
		public static PlayerInfo LocalPlayer => GetPlayer(LocalPlayerId);
		public static List<PlayerInfo> PlayerList { get; } = new List<PlayerInfo>();

		public static List<PlayerSyncObject> PlayerSyncObjects { get; } = new List<PlayerSyncObject>();

		public static PlayerInfo GetPlayer(uint id)
		{
			if (id == uint.MaxValue || id == 0U)
			{
				return default;
			}
			var player = PlayerList.FirstOrDefault(x => x.PlayerId == id);
			if (player != null)
			{
				return player;
			}
			DebugLog.DebugWrite($"Creating player id {id}", MessageType.Info);
			player = new PlayerInfo(id);
			PlayerList.Add(player);
			return player;
		}

		public static void RemovePlayer(uint id)
		{
			DebugLog.DebugWrite($"Removing player {GetPlayer(id).Name} id {id}", MessageType.Info);
			PlayerList.Remove(GetPlayer(id));
		}

		public static void RemoveAllPlayers()
		{
			DebugLog.DebugWrite($"Removing all players.", MessageType.Info);
			PlayerList.Clear();
		}

		public static bool PlayerExists(uint id)
		{
			return id != uint.MaxValue && PlayerList.Any(x => x.PlayerId == id);
		}

		public static void HandleFullStateMessage(PlayerStateMessage message)
		{
			var player = GetPlayer(message.AboutId);
			player.Name = message.PlayerName;
			player.IsReady = message.PlayerReady;
			player.State = message.PlayerState;
			if (LocalPlayer.IsReady)
			{
				player.UpdateStateObjects();
			}
		}

		public static IEnumerable<T> GetSyncObjects<T>() where T : PlayerSyncObject
		{
			return PlayerSyncObjects.OfType<T>().Where(x => x != null);
		}

		public static T GetSyncObject<T>(uint id) where T : PlayerSyncObject
		{
			return GetSyncObjects<T>().FirstOrDefault(x => x != null && x.AttachedNetId == id);
		}

		public static bool IsBelongingToLocalPlayer(uint id)
		{
			return id == LocalPlayerId ||
				PlayerSyncObjects.Any(x => x != null && x.AttachedNetId == id && x.IsLocalPlayer);
		}
	}
}