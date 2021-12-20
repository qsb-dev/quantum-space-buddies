using OWML.Utils;
using QSB.Events;
using QSB.QuantumSync;
using QSB.QuantumSync.WorldObjects;
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

			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}

			var controller = Locator.GetPlayerController();
			if (controller == null)
			{
				return;
			}

			var collidingQuantumObject = controller._collidingQuantumObject;
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
