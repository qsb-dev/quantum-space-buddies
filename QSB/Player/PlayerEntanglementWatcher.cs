using QSB.Messaging;
using QSB.Player.Messages;
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
				var objectId = collidingQuantumObject != null
					? collidingQuantumObject.GetWorldObject<IQSBQuantumObject>().ObjectId
					: -1;

				new PlayerEntangledMessage(objectId).Send();
				_previousCollidingQuantumObject = collidingQuantumObject;
			}
		}
	}
}
