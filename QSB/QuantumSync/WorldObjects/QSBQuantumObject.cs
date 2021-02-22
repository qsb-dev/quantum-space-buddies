using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.QuantumSync.WorldObjects
{
	internal abstract class QSBQuantumObject<T> : WorldObject<T>, IQSBQuantumObject
		where T : MonoBehaviour
	{
		public uint ControllingPlayer { get; set; }
		public bool IsEnabled { get; set; }

		private OnEnableDisableTracker _tracker;

		public override void OnRemoval()
		{
			_tracker.OnEnableEvent -= OnEnable;
			_tracker.OnDisableEvent -= OnDisable;
			Object.Destroy(_tracker);
		}

		public override void Init(T attachedObject, int id)
		{
			_tracker = QSBCore.GameObjectInstance.AddComponent<OnEnableDisableTracker>();
			_tracker.AttachedComponent = AttachedObject;
			_tracker.OnEnableEvent += OnEnable;
			_tracker.OnDisableEvent += OnDisable;
			ControllingPlayer = 0u;
		}

		private void OnEnable()
		{
			IsEnabled = true;
			if (!QSBCore.HasWokenUp && !QSBCore.IsServer)
			{
				return;
			}
			if (ControllingPlayer != 0)
			{
				// controlled by another player, dont care that we activate it
				return;
			}
			var id = QuantumManager.GetId(this);
			// no one is controlling this object right now, request authority
			EventManager.FireEvent(EventNames.QSBQuantumAuthority, id, PlayerManager.LocalPlayerId);
		}

		private void OnDisable()
		{
			IsEnabled = false;
			if (!QSBCore.HasWokenUp && !QSBCore.IsServer)
			{
				return;
			}
			if (ControllingPlayer != PlayerManager.LocalPlayerId)
			{
				// not being controlled by us, don't care if we leave area
				return;
			}
			var id = QuantumManager.GetId(this);
			// send event to other players that we're releasing authority
			EventManager.FireEvent(EventNames.QSBQuantumAuthority, id, 0u);
		}
	}
}
