using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.DreamLantern.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.DreamLantern.WorldObjects;

public class QSBDreamLanternController : WorldObject<DreamLanternController>
{
	public DreamLanternItem DreamLanternItem { get; private set; }

	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);

		// Ghosts don't have the item and instead the effects are controlled by GhostEffects
		if (!IsGhostLantern)
		{
			DreamLanternItem = AttachedObject.GetComponent<DreamLanternItem>();
		}
	}

	public override void SendInitialState(uint to)
	{
		this.SendMessage(new SetLitMessage(AttachedObject._lit) { To = to });
		this.SendMessage(new SetConcealedMessage(AttachedObject._concealed) { To = to });
		this.SendMessage(new SetFocusMessage(AttachedObject._focus) { To = to });
		this.SendMessage(new SetRangeMessage(AttachedObject._minRange, AttachedObject._maxRange) { To = to });
	}

	public bool IsGhostLantern => AttachedObject.name == "GhostLantern"; // it's as shrimple as that
}
