using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.EclipseCodeControllers.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.EclipseCodeControllers.WorldObjects;

public class QSBEclipseCodeController : WorldObject<EclipseCodeController4>
{
	public PlayerInfo PlayerInControl;

	public override void SendInitialState(uint to)
	{
		this.SendMessage(new InitialStateMessage(AttachedObject) { To = to });
		this.SendMessage(new UseControllerMessage(PlayerInControl?.PlayerId ?? 0) { To = to });
	}

	public override async UniTask Init(CancellationToken ct)
	{
		QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
		AttachedObject.gameObject.AddComponent<CodeControllerRemoteUpdater>();
	}

	public override void OnRemoval()
	{
		QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;
		if (AttachedObject)
		{
			UnityEngine.Object.Destroy(AttachedObject.gameObject.GetComponent<CodeControllerRemoteUpdater>());
		}
	}

	private void OnPlayerLeave(PlayerInfo player)
	{
		if (!QSBCore.IsHost)
		{
			return;
		}

		if (PlayerInControl == player)
		{
			this.SendMessage(new UseControllerMessage(false));
		}
	}

	public void SetUser(uint user)
	{
		var player = QSBPlayerManager.GetPlayer(user);
		AttachedObject._interactReceiver.SetInteractionEnabled(user == 0 || player == PlayerInControl);
		PlayerInControl = player;
	}
}
