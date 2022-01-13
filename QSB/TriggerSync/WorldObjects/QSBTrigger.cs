﻿using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.TriggerSync.Messages;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.TriggerSync.WorldObjects
{
	public interface IQSBTrigger : IWorldObject
	{
		List<PlayerInfo> Players { get; }

		void Enter(PlayerInfo player);

		void Exit(PlayerInfo player);
	}

	public abstract class QSBTrigger<TO> : WorldObject<OWTriggerVolume>, IQSBTrigger
	{
		public TO TriggerOwner { get; init; }

		public List<PlayerInfo> Players { get; } = new();

		public override void Init()
		{
			AttachedObject.OnEntry += OnLocalEnter;
			AttachedObject.OnExit += OnLocalExit;

			QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

			QSBCore.UnityEvents.RunWhen(() => WorldObjectManager.AllObjectsReady, () =>
			{
				if (AttachedObject._trackedObjects == null)
				{
					DebugLog.DebugWrite($"{LogName} _trackedObjects == null", MessageType.Warning);
				}
				else if (AttachedObject.IsTrackingObject(Locator.GetPlayerDetector()))
				{
					OnLocalEnter(Locator.GetPlayerDetector());
				}
			});
		}

		public override void OnRemoval()
		{
			AttachedObject.OnEntry -= OnLocalEnter;
			AttachedObject.OnExit -= OnLocalExit;

			QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;
		}

		private void OnLocalEnter(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				((IQSBTrigger)this).SendMessage(new TriggerMessage(true));
			}
		}

		private void OnLocalExit(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				((IQSBTrigger)this).SendMessage(new TriggerMessage(false));
			}
		}

		private void OnPlayerLeave(PlayerInfo player)
		{
			if (Players.Contains(player))
			{
				Exit(player);
			}
		}

		public void Enter(PlayerInfo player)
		{
			if (!Players.SafeAdd(player))
			{
				DebugLog.DebugWrite($"{LogName} + {player.PlayerId}", MessageType.Warning);
				return;
			}

			DebugLog.DebugWrite($"{LogName} + {player.PlayerId}");
			OnEnter(player);
		}

		public void Exit(PlayerInfo player)
		{
			if (!Players.QuickRemove(player))
			{
				DebugLog.DebugWrite($"{LogName} - {player.PlayerId}", MessageType.Warning);
				return;
			}

			DebugLog.DebugWrite($"{LogName} - {player.PlayerId}");
			OnExit(player);
		}

		protected abstract void OnEnter(PlayerInfo player);

		protected abstract void OnExit(PlayerInfo player);
	}
}