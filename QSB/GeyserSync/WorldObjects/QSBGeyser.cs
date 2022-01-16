using QSB.GeyserSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using QuantumUNET;

namespace QSB.GeyserSync.WorldObjects
{
	public class QSBGeyser : WorldObject<GeyserController>
	{
		public override void Init()
		{
			AttachedObject.OnGeyserActivateEvent += () => HandleEvent(true);
			AttachedObject.OnGeyserDeactivateEvent += () => HandleEvent(false);
		}

		private void HandleEvent(bool state)
		{
			if (QNetworkServer.active)
			{
				this.SendMessage(new GeyserMessage(state));
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