using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.Prisoner.WorldObjects;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Prisoner;

public class PrisonerManager : WorldObjectManager
{
	public override bool DlcOnly => true;
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		var director = QSBWorldSync.GetUnityObject<PrisonerDirector>();

		var markers = new List<Transform>()
		{
			director._cellevatorPedestalMarker,
			director._cellevatorWindowMarker,
			director._exitCueMarker,
			director._scanCueMarker,
			director._torchCueMarker,
			director._torchReturnCueMarker
		};

		foreach (var marker in markers)
		{
			marker.gameObject.AddComponent<PrisonerBehaviourCueMarker>();
		}

		QSBWorldSync.Init<QSBPrisonerMarker, PrisonerBehaviourCueMarker>();
		QSBWorldSync.Init<QSBPrisonerBrain, PrisonerBrain>();
		QSBWorldSync.Init<QSBPrisonCellElevator, PrisonCellElevator>();
	}
}
