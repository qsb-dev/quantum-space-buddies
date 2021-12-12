using OWML.Utils;
using QSB.Events;
using QSB.QuantumSync;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Player
{
	internal class PlayerEntanglementWatcher : MonoBehaviour
	{
		private QuantumObject _previousCollidingQuantumObject;

		private void Update()
		{
			if (!QSBCore.IsInMultiplayer)
			{
				return;
			}

			var controller = Locator.GetPlayerController();
			if (controller == null)
			{
				return;
			}

			var collidingQuantumObject = controller.GetValue<QuantumObject>("_collidingQuantumObject");
			if (_previousCollidingQuantumObject != collidingQuantumObject)
			{
				var objectIndex = (collidingQuantumObject != null)
					? QSBWorldSync.GetWorldFromUnity<IQSBQuantumObject>(collidingQuantumObject).ObjectId
					: -1;

				QSBEventManager.FireEvent(
					EventNames.QSBPlayerEntangle,
					objectIndex);
				_previousCollidingQuantumObject = collidingQuantumObject;
			}
		}
	}
}
