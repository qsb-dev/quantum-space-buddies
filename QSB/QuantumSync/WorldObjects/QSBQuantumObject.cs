using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.QuantumSync.WorldObjects
{
	internal abstract class QSBQuantumObject<T> : WorldObject<T> where T : UnityEngine.Object
	{
		private const uint NoControllerValue = uint.MaxValue;

		public uint ControllingPlayer = NoControllerValue;

		public override void Init(T attachedObject, int id)
		{
			var tracker = (AttachedObject as GameObject).AddComponent<OnEnableDisableTracker>();
			tracker.OnEnableEvent += OnEnable;
			tracker.OnDisableEvent += OnDisable;
			DebugLog.DebugWrite($"Finish setup of {attachedObject.name}");
		}

		private void OnEnable()
		{
			if (ControllingPlayer != uint.MaxValue && !QSBCore.IsServer)
			{
				// controlled by another player, dont care that we activate it (unless we're the server!)
				return;
			}
			// no one is controlling this object right now (or we're the server, and we want to take ownership), request authority
			GlobalMessenger<uint>.FireEvent(EventNames.QSBQuantumAuthority, QSBPlayerManager.LocalPlayerId);
		}

		private void OnDisable()
		{
			if (ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				// not being controlled by us, don't care if we leave area
				return;
			}
			// send event to other players that we're releasing authority
			GlobalMessenger<uint>.FireEvent(EventNames.QSBQuantumAuthority, NoControllerValue);
		}
	}
}
