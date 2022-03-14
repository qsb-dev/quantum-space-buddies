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
		=> this.SendMessage(new InitialStateMessage(AttachedObject) { To = to });

	public override async UniTask Init(CancellationToken ct)
	{
		AttachedObject.gameObject.AddComponent<CodeControllerRemoteUpdater>();
	}

	public override void OnRemoval()
	{
		UnityEngine.Object.Destroy(AttachedObject.gameObject.GetComponent<CodeControllerRemoteUpdater>());
	}

	public void SetUser(uint user)
	{
		var player = QSBPlayerManager.GetPlayer(user);
		AttachedObject._interactReceiver.SetInteractionEnabled(user == 0 || player == PlayerInControl);
		PlayerInControl = player;
	}
}
