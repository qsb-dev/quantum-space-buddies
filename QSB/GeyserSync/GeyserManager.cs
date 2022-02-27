using Cysharp.Threading.Tasks;
using QSB.GeyserSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.GeyserSync
{
	public class GeyserManager : WorldObjectManager
	{
		public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
			=> QSBWorldSync.Init<QSBGeyser, GeyserController>();
	}
}