using QSB.Events;
using QSB.Player;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.QuantumSync.WorldObjects
{
	internal abstract class QSBQuantumObject<T> : WorldObject<T>, IQSBQuantumObject
		where T : MonoBehaviour
	{
		public uint ControllingPlayer { get; set; }
		public bool IsEnabled { get; set; }

		public override void Init(T attachedObject, int id) => ControllingPlayer = 0u;

		public void Enable()
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
			var id = QuantumManager.Instance.GetId(this);
			// no one is controlling this object right now, request authority
			QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, id, QSBPlayerManager.LocalPlayerId);
		}

		public void Disable()
		{
			IsEnabled = false;
			if (!QSBCore.HasWokenUp && !QSBCore.IsServer)
			{
				return;
			}
			if (ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				// not being controlled by us, don't care if we leave area
				return;
			}
			var id = QuantumManager.Instance.GetId(this);
			// send event to other players that we're releasing authority
			QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, id, 0u);
		}
	}
}
