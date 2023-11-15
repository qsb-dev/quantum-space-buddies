using Cysharp.Threading.Tasks;
using QSB.GeyserSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using System.Threading;

namespace QSB.GeyserSync.WorldObjects;

public class QSBGeyser : WorldObject<GeyserController>
{
	public override async UniTask Init(CancellationToken ct)
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