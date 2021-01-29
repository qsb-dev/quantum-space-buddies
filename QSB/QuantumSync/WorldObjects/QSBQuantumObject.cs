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

		public override void Init(T attachedObject, int id)
		{
			var tracker = (AttachedObject as Behaviour).gameObject.AddComponent<OnEnableDisableTracker>();
			tracker.AttachedComponent = AttachedObject.gameObject.GetComponent<T>();
			tracker.OnEnableEvent += OnEnable;
			tracker.OnDisableEvent += OnDisable;
			ControllingPlayer = QSBCore.IsServer ? 1u : 0u;
		}

		private void OnEnable()
		{
			if (ControllingPlayer != 0)
			{
				// controlled by another player, dont care that we activate it
				return;
			}
			var id = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>().ToList().IndexOf(this);
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
			// send event to other players that we're releasing authority
			GlobalMessenger<int, uint>.FireEvent(EventNames.QSBQuantumAuthority, id, 0);
			ControllingPlayer = 0;
		}
	}
}
