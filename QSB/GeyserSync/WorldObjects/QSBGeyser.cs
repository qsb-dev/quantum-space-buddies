using QSB.GeyserSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.GeyserSync.WorldObjects
{
	public class QSBGeyser : WorldObject<GeyserController>
	{
		public override void Init()
		{
			if (QSBCore.IsHost)
			{
				AttachedObject.OnGeyserActivateEvent += OnActivate;
				AttachedObject.OnGeyserDeactivateEvent += OnDeactivate;
			}
		}

		public override void OnRemoval()
		{
			if (QSBCore.IsHost)
			{
				AttachedObject.OnGeyserActivateEvent -= OnActivate;
				AttachedObject.OnGeyserDeactivateEvent -= OnDeactivate;
			}
		}

		public override void SendInitialState(uint to)
		{
			if (QSBCore.IsHost)
			{
				this.SendMessage(new GeyserMessage(AttachedObject._isActive));
			}
		}

		private void OnActivate() => this.SendMessage(new GeyserMessage(true));
		private void OnDeactivate() => this.SendMessage(new GeyserMessage(false));

		public void SetState(bool state)
		{
			if (AttachedObject._isActive == state)
			{
				return;
			}

			if (state)
			{
				AttachedObject.ActivateGeyser();
			}
			else
			{
				AttachedObject.DeactivateGeyser();
			}
		}
	}
}
