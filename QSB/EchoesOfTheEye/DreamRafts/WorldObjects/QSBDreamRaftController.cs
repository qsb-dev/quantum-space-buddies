using Cysharp.Threading.Tasks;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Threading;
using UnityEngine;

namespace QSB.EchoesOfTheEye.DreamRafts.WorldObjects;

public class QSBDreamRaftController : WorldObject<DreamRaftController>
{
	public override void SendInitialState(uint to) { }

	public override async UniTask Init(CancellationToken ct) =>
		EnableDisableDetector.Add(AttachedObject.gameObject, this);
}

public class QSBSealRaftController : WorldObject<SealRaftController>
{
	public override void SendInitialState(uint to) { }

	public override async UniTask Init(CancellationToken ct) =>
		EnableDisableDetector.Add(AttachedObject.gameObject, this);
}

public class EnableDisableDetector : MonoBehaviour
{
	public static void Add(GameObject go, object linkedObject)
	{
		if (go.activeSelf)
		{
			go.SetActive(false);
			go.AddComponent<EnableDisableDetector>()._linkedObject = linkedObject;
			go.SetActive(true);
		}
		else
		{
			go.AddComponent<EnableDisableDetector>()._linkedObject = linkedObject;
		}
	}

	private object _linkedObject;

	private void Awake()
	{
		var body = this.GetAttachedOWRigidbody();
		if (body)
		{
			DebugLog.DebugWrite($"{_linkedObject} suspended = {body.IsSuspended()}");
			body.OnSuspendOWRigidbody += _ => DebugLog.DebugWrite($"{_linkedObject} suspend\n{Environment.StackTrace}");
			body.OnPreUnsuspendOWRigidbody += _ => DebugLog.DebugWrite($"{_linkedObject} unsuspend\n{Environment.StackTrace}");
		}
	}

	private void OnEnable() => DebugLog.DebugWrite($"{_linkedObject} enable");
	private void OnDisable() => DebugLog.DebugWrite($"{_linkedObject} disable");
}
