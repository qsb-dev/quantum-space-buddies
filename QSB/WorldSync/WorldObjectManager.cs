using System;
using System.Collections.Generic;
using OWML.Common;
using QSB.Player;
using QSB.Utility;
using UnityEngine;

namespace QSB.WorldSync
{
	public abstract class WorldObjectManager : MonoBehaviour
	{
		private static readonly List<WorldObjectManager> _managers = new();

		public static bool AllAdded { get; private set; }
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

		public static void SetNotReady()
		{
			AllAdded = false;
			AllReady = false;
		}

		private void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool inUniverse)
		{
			AllAdded = false;
			AllReady = false;
		}

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
			_numManagersReadying = 0;
			_numObjectsReadying = 0;
			AllAdded = false;
			AllReady = false;
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

			QSBCore.UnityEvents.RunWhen(() => _numManagersReadying == 0, () =>
			{
				AllAdded = true;
				DebugLog.DebugWrite("World Objects added.", MessageType.Success);
				QSBCore.UnityEvents.RunWhen(() => _numObjectsReadying == 0, () =>
				{
					AllReady = true;
					DebugLog.DebugWrite("World Objects ready.", MessageType.Success);
				});
			});
		}

		protected abstract void RebuildWorldObjects(OWScene scene);

		private static uint _numManagersReadying;
		internal static uint _numObjectsReadying;

		/// indicates that this won't become ready immediately
		protected void StartDelayedReady() => _numManagersReadying++;

		/// indicates that this is now ready
		protected void FinishDelayedReady() => _numManagersReadying--;
	}
}
