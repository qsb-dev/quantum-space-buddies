using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.DreamLantern.WorldObjects;

public class QSBDreamLanternController : WorldObject<DreamLanternController>
{
	public DreamLanternItem DreamLanternItem { get; private set; }

	public override async UniTask Init(CancellationToken ct)
	{
		// Ghosts don't have the item and instead the effects are controlled by GhostEffects
		if (!IsGhostLantern)
		{
			DreamLanternItem = AttachedObject.GetComponent<DreamLanternItem>();
		}
	}

	public bool IsGhostLantern => AttachedObject.name == "GhostLantern"; // it's as shrimple as that
}
