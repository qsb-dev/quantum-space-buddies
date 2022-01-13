using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.TriggerSync
{
	public class TriggerLink : IDisposable
	{
		public string LogName =>
			$"trigger {QSBPlayerManager.LocalPlayerId}.{_id}.{(_trigger ? _trigger.name : "null")}";

		public readonly List<PlayerInfo> Players = new();

		private readonly int _id;
		private readonly OWTriggerVolume _trigger;

		public TriggerLink(int id, OWTriggerVolume trigger)
		{
			_id = id;
			_trigger = trigger;

			_trigger.OnEntry += OnEntry;
			_trigger.OnExit += OnExit;

			if (_trigger._trackedObjects != null && _trigger.IsTrackingObject(Locator.GetPlayerDetector()))
			{
				OnEntry(Locator.GetPlayerDetector());
			}

			DebugLog.DebugWrite($"{LogName} created");
		}

		public void Dispose()
		{
			_trigger.OnEntry -= OnEntry;
			_trigger.OnExit -= OnExit;

			DebugLog.DebugWrite($"{LogName} disposed");
		}

		private void OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				new TriggerMessage(_id, true).Send();
			}
		}

		private void OnExit(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				new TriggerMessage(_id, false).Send();
			}
		}

		public void Enter(PlayerInfo player)
		{
			if (!Players.SafeAdd(player))
			{
				DebugLog.DebugWrite($"{LogName} already added {player.PlayerId}", MessageType.Warning);
				return;
			}

			DebugLog.DebugWrite($"{LogName} + {player.PlayerId}");
		}

		public void Exit(PlayerInfo player)
		{
			if (!Players.QuickRemove(player))
			{
				DebugLog.DebugWrite($"{LogName} already removed {player.PlayerId}", MessageType.Warning);
				return;
			}

			DebugLog.DebugWrite($"{LogName} - {player.PlayerId}");
		}
	}
}
