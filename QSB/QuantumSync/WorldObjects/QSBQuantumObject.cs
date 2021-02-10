using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
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
			ControllingPlayer = 1u;
		}

		private void OnEnable()
		{
			IsEnabled = true;
			if (ControllingPlayer != 0)
			{
				// controlled by another player, dont care that we activate it
				return;
			}
			if (!QSBCore.IsInMultiplayer)
			{
				return;
			}
			var id = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ToList().IndexOf(this);
			// no one is controlling this object right now, request authority
			QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, id, QSBPlayerManager.LocalPlayerId);
		}

		private void OnDisable()
		{
			IsEnabled = false;
			if (ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				// not being controlled by us, don't care if we leave area
				return;
			}
			if (!QSBCore.IsInMultiplayer)
			{
				return;
			}
			var id = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ToList().IndexOf(this);
			// send event to other players that we're releasing authority
			QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, id, 0);
		}
	}
}
