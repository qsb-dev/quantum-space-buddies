using OWML.Utils;
using QSB.Events;
using QSB.QuantumSync;
using UnityEngine;

namespace QSB.Player
{
	internal class PlayerEntanglementWatcher : MonoBehaviour
	{
		private QuantumObject _previousCollidingQuantumObject;

		private void Update()
		{
			var controller = Locator.GetPlayerController();
			if (controller == null)
			{
				return;
			}
			var collidingQuantumObject = controller.GetValue<QuantumObject>("_collidingQuantumObject");
			if (_previousCollidingQuantumObject != collidingQuantumObject)
			{
				var objectIndex = (collidingQuantumObject != null)
					? QuantumManager.GetId(QuantumManager.GetObject(collidingQuantumObject))
					: -1;

				QSBEventManager.FireEvent(
					EventNames.QSBPlayerEntangle,
					objectIndex);
				_previousCollidingQuantumObject = collidingQuantumObject;
			}
		}
	}
}
