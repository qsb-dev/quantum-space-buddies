using QSB.Events;
using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.GeyserSync
{
	public class QSBGeyser : WorldObject<GeyserController>
	{
		private GeyserController _geyserController;

		public override void Init(GeyserController geyserController, int id)
		{
			ObjectId = id;
			_geyserController = geyserController;
			_geyserController.OnGeyserActivateEvent += () => HandleEvent(true);
			_geyserController.OnGeyserDeactivateEvent += () => HandleEvent(false);
		}

		private void HandleEvent(bool state)
		{
			if (QNetworkServer.active)
			{
				GlobalMessenger<int, bool>.FireEvent(EventNames.QSBGeyserState, ObjectId, state);
			}
		}

		public void SetState(bool state)
		{
			if (state)
			{
				_geyserController?.ActivateGeyser();
				return;
			}
			_geyserController?.DeactivateGeyser();
		}
	}
}