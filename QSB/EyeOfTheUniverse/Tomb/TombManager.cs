using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EyeOfTheUniverse.Tomb;

public class TombManager : WorldObjectManager
{
	public override bool DlcOnly => true;
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Eye;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		// sike!! no worldobjects here
		var tomb = QSBWorldSync.GetUnityObject<EyeTombController>();
		tomb.gameObject.AddComponent<EyeTombWatcher>();
	}
}
