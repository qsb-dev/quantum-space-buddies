using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.EchoesOfTheEye.RaftSync;

public class RaftManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public static readonly List<RaftController> Rafts = new();
	public static DamRaftLift DamRaftLift { get; private set; }

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		Rafts.Clear();
		Rafts.AddRange(QSBWorldSync.GetUnityObjects<RaftController>().SortDeterministic());
		QSBWorldSync.Init<QSBRaft, RaftController>(Rafts);

		QSBWorldSync.Init<QSBRaftDock, RaftDock>();
		DamRaftLift = QSBWorldSync.GetUnityObjects<DamRaftLift>().First();
	}

	public static void StartLiftingRaft(QSBRaft qsbRaft)
	{
		DamRaftLift._raft = qsbRaft.AttachedObject;
		DamRaftLift._raft.OnArriveAtTarget += DamRaftLift.OnArriveAtTarget;
		DamRaftLift.GetAlignDestination().localEulerAngles = Vector3.zero;
		var to = DamRaftLift.GetAlignDestination().InverseTransformDirection(DamRaftLift._raft.transform.forward);
		to.y = 0f;
		var num = OWMath.Angle(Vector3.forward, to, Vector3.up);
		num = OWMath.RoundToNearestMultiple(num, 90f);
		DamRaftLift.GetAlignDestination().localEulerAngles = new Vector3(0f, num, 0f);
		var vector = DamRaftLift.GetAlignDestination().position - DamRaftLift._raft.GetBody().GetPosition();
		vector = Vector3.Project(vector, DamRaftLift._raft.transform.up);
		var position = DamRaftLift.GetAlignDestination().position - DamRaftLift.GetAlignDestination().up * vector.magnitude;
		DamRaftLift._raft.MoveToTarget(position, DamRaftLift.GetAlignDestination().rotation, DamRaftLift._raftAlignSpeed, false);
		DamRaftLift._oneShotAudio.PlayOneShot(AudioType.Raft_Reel_Start);
		DamRaftLift._loopingAudio.FadeIn(0.2f);
		DamRaftLift._state = RaftCarrier.DockState.AligningBelow;

		DamRaftLift.enabled = true;
		foreach (var liftNode in DamRaftLift._liftNodes)
		{
			liftNode.localEulerAngles = DamRaftLift.GetAlignDestination().localEulerAngles;
		}

		DamRaftLift._nodeIndex = 1;
		DamRaftLift._raftDockLights.SetLightsActivation(true);
	}

	public static void StopLiftingRaft(bool damBroken)
	{
		if (DamRaftLift._raft == null)
		{
			return;
		}

		if (damBroken)
		{
			DamRaftLift._raft.OnArriveAtTarget -= DamRaftLift.OnArriveAtTarget;
			DamRaftLift._raft.StopMovingToTarget();
			DamRaftLift._craneHookRoot.parent = null;
			foreach (var hookRenderer in DamRaftLift._hookRenderers)
			{
				hookRenderer.SetActivation(false);
			}

			DamRaftLift._raft = null;
			DamRaftLift._trigger.SetTriggerActivation(false);
		}
		else
		{
			DamRaftLift._nodeIndex = 0;
			DamRaftLift.PlayHookAnimation();
			DamRaftLift._raft.StopMovingToTarget();
			DamRaftLift._raft.GetOneShotAudio().PlayOneShot(AudioType.Raft_Release);
			DamRaftLift._raft.OnArriveAtTarget -= DamRaftLift.OnArriveAtTarget;
			DamRaftLift._raft = null;
			DamRaftLift._state = RaftCarrier.DockState.ResettingHook;
		}
	}
}
