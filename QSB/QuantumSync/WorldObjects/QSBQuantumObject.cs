using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
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
			if (ControllingPlayer != 0)
			{
				// controlled by another player, dont care that we activate it
				return;
			}
			var id = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ToList().IndexOf(this);
			//DebugLog.DebugWrite($"ON ENABLE {(this as WorldObject<T>).AttachedObject.name} ({id})");
			// no one is controlling this object right now, request authority
			GlobalMessenger<int, uint>.FireEvent(EventNames.QSBQuantumAuthority, id, QSBPlayerManager.LocalPlayerId);
			ControllingPlayer = QSBPlayerManager.LocalPlayerId;
		}

		private void OnDisable()
		{
			if (ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				// not being controlled by us, don't care if we leave area
				return;
			}
			var id = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ToList().IndexOf(this);
			//DebugLog.DebugWrite($"ON DISABLE {(this as WorldObject<T>).AttachedObject.name} ({id})");
			// send event to other players that we're releasing authority
			GlobalMessenger<int, uint>.FireEvent(EventNames.QSBQuantumAuthority, id, 0);
			ControllingPlayer = 0;
		}
	}
}
