using OWML.Common;
using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.WorldSync
{
	public enum WorldObjectType
	{
		Both,
		SolarSystem,
		Eye
	}

	public abstract class WorldObjectManager : MonoBehaviour
	{
		private static readonly List<WorldObjectManager> _managers = new();

		/// <summary>
		/// Set when all WorldObjectManagers have called Init() on all their objects (AKA all the objects are created)
		/// </summary>
		public static bool AllObjectsAdded { get; private set; }

		/// <summary>
		/// Set when all WorldObjects have finished running Init()
		/// </summary>
		public static bool AllObjectsReady { get; private set; }

		/// <summary>
		/// when the scene does not match the type, this manager will not build its world objects
		/// </summary>
		public abstract WorldObjectType WorldObjectType { get; }

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
			AllObjectsAdded = false;
			AllObjectsReady = false;
		}

		private void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool inUniverse)
		{
			AllObjectsAdded = false;
			AllObjectsReady = false;
		}

		public static void Rebuild(OWScene scene)
		{
			if (!QSBNetworkManager.Instance.IsReady)
			{
				DebugLog.ToConsole($"Warning - Tried to rebuild WorldObjects when Network Manager not ready! Building when ready...", MessageType.Warning);
				QSBCore.UnityEvents.RunWhen(() => QSBNetworkManager.Instance.IsReady, () => Rebuild(scene));
				return;
			}

			if (QSBPlayerManager.LocalPlayerId == uint.MaxValue)
			{
				DebugLog.ToConsole($"Warning - Tried to rebuild WorldObjects when LocalPlayer is not ready! Building when ready...", MessageType.Warning);
				QSBCore.UnityEvents.RunWhen(() => QSBPlayerManager.LocalPlayerId != uint.MaxValue, () => Rebuild(scene));
				return;
			}

			DoRebuild(scene);
		}

		private static void DoRebuild(OWScene scene)
		{
			QSBWorldSync.RemoveWorldObjects();
			_numManagersReadying = 0;
			_numObjectsReadying = 0;
			AllObjectsAdded = false;
			AllObjectsReady = false;
			foreach (var manager in _managers)
			{
				switch (manager.WorldObjectType)
				{
					case WorldObjectType.SolarSystem when QSBSceneManager.CurrentScene != OWScene.SolarSystem:
					case WorldObjectType.Eye when QSBSceneManager.CurrentScene != OWScene.EyeOfTheUniverse:
						DebugLog.DebugWrite($"skipping {manager.GetType().Name} as it is type {manager.WorldObjectType} and scene is {QSBSceneManager.CurrentScene}");
						continue;
				}

				try
				{
					DebugLog.DebugWrite($"Rebuilding {manager.GetType().Name}", MessageType.Info);
					manager.RebuildWorldObjects(scene);
				}
				catch (Exception ex)
				{
					DebugLog.ToConsole($"Exception - Exception when trying to rebuild WorldObjects of manager {manager.GetType().Name} : {ex.Message} Stacktrace :\r\n{ex.StackTrace}", MessageType.Error);
				}
			}

			QSBCore.UnityEvents.RunWhen(() => _numManagersReadying == 0, () =>
			{
				AllObjectsAdded = true;
				DebugLog.DebugWrite("World Objects added.", MessageType.Success);
				QSBCore.UnityEvents.RunWhen(() => _numObjectsReadying == 0, () =>
				{
					AllObjectsReady = true;
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

		private void OnGUI()
		{
			if (!QSBCore.ShowDebugLabels)
			{
				return;
			}

			foreach (var obj in QSBWorldSync.GetWorldObjects())
			{
				if (obj.ShouldDisplayLabel)
				{
					DebugGUI.DrawLabel(obj.ReturnObject().transform, obj.ReturnLabel());
				}
			}
		}
	}
}
