﻿using QSB.Events;
using QSB.GeyserSync.Events;
using QSB.Messaging;
using QSB.WorldSync;
using QuantumUNET;

namespace QSB.GeyserSync.WorldObjects
{
	public class QSBGeyser : WorldObject<GeyserController>
	{
		public override void Init(GeyserController geyserController, int id)
		{
			ObjectId = id;
			AttachedObject = geyserController;
			AttachedObject.OnGeyserActivateEvent += () => HandleEvent(true);
			AttachedObject.OnGeyserDeactivateEvent += () => HandleEvent(false);
		}

		private void HandleEvent(bool state)
		{
			if (QSBCore.IsHost)
			{
				this.SendMessage(new GeyserMessage
				{
					Value = state
				});
				// QSBEventManager.FireEvent(EventNames.QSBGeyserState, ObjectId, state);
			}
		}

		public void SetState(bool state)
		{
			if (state)
			{
				AttachedObject?.ActivateGeyser();
				return;
			}

			AttachedObject?.DeactivateGeyser();
		}
	}
}