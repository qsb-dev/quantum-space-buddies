using Cysharp.Threading.Tasks;
using QSB.WorldSync.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QSB.WorldSync;

internal class MiscManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;
	public override bool DlcOnly => false;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		await UniTask.WaitUntil(() => LateInitializerManager.isDoneInitializing, cancellationToken: ct);

		QSBWorldSync.Init<QSBOWRigidbody, OWRigidbody>(typeof(PlayerBody), typeof(ShipBody));
	}
}
