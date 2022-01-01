using OWML.Common;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Player.TransformSync;
using QSB.Tools.FlashlightTool;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.Player
{
	public static class QSBPlayerManager
	{
		public static uint LocalPlayerId
		{
			get
			{
				var localInstance = PlayerTransformSync.LocalInstance;
				if (localInstance == null)
				{
					DebugLog.ToConsole($"Error - Trying to get LocalPlayerId when the local PlayerTransformSync instance is null." +
						$"{Environment.NewLine} Stacktrace : {Environment.StackTrace} ", MessageType.Error);
					return uint.MaxValue;
				}

				if (localInstance.NetIdentity == null)
				{
					DebugLog.ToConsole($"Error - Trying to get LocalPlayerId when the local PlayerTransformSync instance's QNetworkIdentity is null.", MessageType.Error);
					return uint.MaxValue;
				}

				return localInstance.NetIdentity.NetId.Value;
			}
		}

		/// <summary>
		/// called right before player is removed
		/// </summary>
		public static Action<uint> OnRemovePlayer;
		/// <summary>
		/// called right after player is added
		/// </summary>
		public static Action<uint> OnAddPlayer;

		public static PlayerInfo LocalPlayer => GetPlayer(LocalPlayerId);
		public static List<PlayerInfo> PlayerList { get; } = new List<PlayerInfo>();

		private static readonly List<PlayerSyncObject> PlayerSyncObjects = new();

		public static PlayerInfo GetPlayer(uint id)
		{
			if (id is uint.MaxValue or 0U)
			{
				return default;
			}

			var player = PlayerList.FirstOrDefault(x => x.PlayerId == id);
			if (player == null)
			{
				DebugLog.ToConsole($"Error - Player with id {id} does not exist! Stacktrace : {Environment.StackTrace}", MessageType.Error);
				return default;
			}

			return player;
		}

		public static void AddPlayer(uint id)
		{
			DebugLog.DebugWrite($"Create Player : id<{id}>", MessageType.Info);
			var player = new PlayerInfo(id);
			PlayerList.Add(player);
			OnAddPlayer?.Invoke(id);
		}

		public static void RemovePlayer(uint id)
		{
			DebugLog.DebugWrite($"Remove Player : id<{id}>", MessageType.Info);
			PlayerList.RemoveAll(x => x.PlayerId == id);
		}

		public static bool PlayerExists(uint id) =>
			id != uint.MaxValue && PlayerList.Any(x => x.PlayerId == id);

		public static IEnumerable<T> GetSyncObjects<T>() where T : PlayerSyncObject =>
			PlayerSyncObjects.OfType<T>().Where(x => x != null);

		public static T GetSyncObject<T>(uint id) where T : PlayerSyncObject =>
			GetSyncObjects<T>().FirstOrDefault(x => x != null && x.AttachedNetId == id);

		public static void AddSyncObject(PlayerSyncObject obj) => PlayerSyncObjects.Add(obj);

		public static void RemoveSyncObject(PlayerSyncObject obj) => PlayerSyncObjects.Remove(obj);

		public static List<PlayerInfo> GetPlayersWithCameras(bool includeLocalCamera = true)
		{
			var cameraList = PlayerList.Where(x => x.Camera != null && x.PlayerId != LocalPlayerId).ToList();
			if (includeLocalCamera
				&& LocalPlayer != default
				&& LocalPlayer.Camera != null)
			{
				cameraList.Add(LocalPlayer);
			}
			else if (includeLocalCamera && (LocalPlayer == default || LocalPlayer.Camera == null))
			{
				if (LocalPlayer == default)
				{
					DebugLog.ToConsole($"Error - LocalPlayer is null.", MessageType.Error);
					return cameraList;
				}

				DebugLog.ToConsole($"Error - LocalPlayer.Camera is null.", MessageType.Error);
				LocalPlayer.Camera = Locator.GetPlayerCamera();
			}

			return cameraList;
		}

		public static Tuple<Flashlight, IEnumerable<QSBFlashlight>> GetPlayerFlashlights()
			=> new(Locator.GetFlashlight(), PlayerList.Where(x => x.FlashLight != null).Select(x => x.FlashLight));

		public static void ShowAllPlayers()
			=> PlayerList.Where(x => x != LocalPlayer && x.DitheringAnimator != null).ToList().ForEach(x => x.DitheringAnimator.SetVisible(true, 0.5f));

		public static void HideAllPlayers()
			=> PlayerList.Where(x => x != LocalPlayer && x.DitheringAnimator != null).ToList().ForEach(x => x.DitheringAnimator.SetVisible(true, 0.5f));

		public static PlayerInfo GetClosestPlayerToWorldPoint(Vector3 worldPoint, bool includeLocalPlayer) => includeLocalPlayer
				? GetClosestPlayerToWorldPoint(PlayerList, worldPoint)
				: GetClosestPlayerToWorldPoint(PlayerList.Where(x => x != LocalPlayer).ToList(), worldPoint);

		public static PlayerInfo GetClosestPlayerToWorldPoint(List<PlayerInfo> playerList, Vector3 worldPoint)
		{
			if (playerList == null)
			{
				DebugLog.ToConsole($"Error - Cannot get closest player from null player list.", MessageType.Error);
				return null;
			}

			if (playerList.Count == 0)
			{
				DebugLog.ToConsole($"Error - Cannot get closest player from empty player list.", MessageType.Error);
				return null;
			}

			return playerList.Where(x => x.IsReady && x.Body != null).OrderBy(x => Vector3.Distance(x.Body.transform.position, worldPoint)).FirstOrDefault();
		}

		public static IEnumerable<Tuple<PlayerInfo, IQSBOWItem>> GetPlayerCarryItems()
			=> PlayerList.Select(x => new Tuple<PlayerInfo, IQSBOWItem>(x, x.HeldItem));
	}
}