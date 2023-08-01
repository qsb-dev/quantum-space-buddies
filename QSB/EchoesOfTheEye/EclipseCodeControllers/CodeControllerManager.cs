using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.EclipseCodeControllers.WorldObjects;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.EclipseCodeControllers;

public class CodeControllerManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBEclipseCodeController, EclipseCodeController4>();
	}
}
