using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.QuantumSync.WorldObjects
{
	internal abstract class QSBQuantumObject<T> : WorldObject<T>, IQSBQuantumObject where T : UnityEngine.Object
	{
		public uint ControllingPlayer { get; set; }

		public override void Init(T attachedObject, int id)
		{
			var tracker = (AttachedObject as Component).gameObject.AddComponent<OnEnableDisableTracker>();
			tracker.OnEnableEvent += OnEnable;
			tracker.OnDisableEvent += OnDisable;
			ControllingPlayer = 0;
		}

		private void OnEnable()
		{
			if (ControllingPlayer != 0 && !QSBCore.IsServer)
			{
				// controlled by another player, dont care that we activate it (unless we're the server!)
				return;
			}
			// no one is controlling this object right now (or we're the server, and we want to take ownership), request authority
			GlobalMessenger<int, uint>.FireEvent(EventNames.QSBQuantumAuthority, ObjectId, QSBPlayerManager.LocalPlayerId);
		}

		private void OnDisable()
		{
			if (ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				// not being controlled by us, don't care if we leave area
				return;
			}
			// send event to other players that we're releasing authority
			GlobalMessenger<int, uint>.FireEvent(EventNames.QSBQuantumAuthority, ObjectId, 0);
		}
	}
}
