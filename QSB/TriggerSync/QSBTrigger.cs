using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.TriggerSync
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
			AttachedObject.OnEntry += OnEntry;
			AttachedObject.OnExit += OnExit;

			QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;

			QSBCore.UnityEvents.RunWhen(() => WorldObjectManager.AllObjectsReady, () =>
			{
				if (AttachedObject._trackedObjects == null)
				{
					DebugLog.DebugWrite($"{LogName} tracked objects == null", MessageType.Warning);
				}
				else if (AttachedObject._trackedObjects.Contains(Locator.GetPlayerDetector()))
				{
					((IQSBTrigger)this).SendMessage(new TriggerMessage(true));
				}
			});
		}

		public override void OnRemoval()
		{
			AttachedObject.OnEntry -= OnEntry;
			AttachedObject.OnExit -= OnExit;

			QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;
		}

		private void OnPlayerLeave(PlayerInfo player)
		{
			if (Players.Contains(player))
			{
				Exit(player);
			}
		}

		private void OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				((IQSBTrigger)this).SendMessage(new TriggerMessage(true));
			}
		}

		private void OnExit(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				((IQSBTrigger)this).SendMessage(new TriggerMessage(false));
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
		}

		public void Exit(PlayerInfo player)
		{
			if (!Players.QuickRemove(player))
			{
				DebugLog.DebugWrite($"{LogName} - {player.PlayerId}", MessageType.Warning);
				return;
			}

			DebugLog.DebugWrite($"{LogName} - {player.PlayerId}");
		}
	}

	public class QSBCharacterTrigger : QSBTrigger<CharacterAnimController>
	{
	}

	public class QSBSolanumTrigger : QSBTrigger<NomaiConversationManager>
	{
	}

	public class QSBVesselCageTrigger : QSBTrigger<VesselWarpController>
	{
	}

	public class QSBMaskZoneTrigger : QSBTrigger<MaskZoneController>
	{
	}
}
