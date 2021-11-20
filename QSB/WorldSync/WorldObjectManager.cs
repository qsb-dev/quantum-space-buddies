using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.WorldSync
{
	public abstract class WorldObjectManager : MonoBehaviour
	{
		private static readonly List<WorldObjectManager> _managers = new List<WorldObjectManager>();

		public static bool AllReady { get; private set; }

		public virtual void Awake()
		{
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
			_managers.Add(this);
		}

		public virtual void OnDestroy()
		{
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
			_managers.Remove(this);
		}

		public static void SetNotReady() => AllReady = false;

		private void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool inUniverse) => AllReady = false;

		public static void Rebuild(OWScene scene)
		{
			if (!QSBNetworkManager.Instance.IsReady)
			{
				DebugLog.ToConsole($"Warning - Tried to rebuild WorldObjects when Network Manager not ready!", OWML.Common.MessageType.Warning);
				QSBCore.UnityEvents.RunWhen(() => QSBNetworkManager.Instance.IsReady, () => Rebuild(scene));
				return;
			}

			if (QSBPlayerManager.LocalPlayerId == uint.MaxValue)
			{
				DebugLog.ToConsole($"Warning - Tried to rebuild WorldObjects when LocalPlayer is not ready!", OWML.Common.MessageType.Warning);
				QSBCore.UnityEvents.RunWhen(() => QSBPlayerManager.LocalPlayerId != uint.MaxValue, () => Rebuild(scene));
				return;
			}

			if (QSBPlayerManager.LocalPlayer.IsReady)
			{
				DoRebuild(scene);
				return;
			}

			QSBCore.UnityEvents.RunWhen(() => QSBPlayerManager.LocalPlayer.IsReady, () => DoRebuild(scene));
		}

		private static void DoRebuild(OWScene scene)
		{
			foreach (var manager in _managers)
			{
				try
				{
					manager.RebuildWorldObjects(scene);
				}
				catch (Exception ex)
				{
					DebugLog.ToConsole($"Exception - Exception when trying to rebuild WorldObjects of manager {manager.GetType().Name} : {ex.Message} Stacktrace :\r\n{ex.StackTrace}", OWML.Common.MessageType.Error);
				}
			}

			QSBCore.UnityEvents.FireInNUpdates(DoPostInit, 1);
		}

		private static void DoPostInit()
		{
			AllReady = true;
			var allWorldObjects = QSBWorldSync.GetWorldObjects<IWorldObject>();
			foreach (var worldObject in allWorldObjects)
			{
				worldObject.PostInit();
			}
		}

		protected abstract void RebuildWorldObjects(OWScene scene);
	}
}
