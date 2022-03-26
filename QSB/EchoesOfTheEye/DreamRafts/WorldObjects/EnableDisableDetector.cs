using QSB.Utility;
using System;
using UnityEngine;

namespace QSB.EchoesOfTheEye.DreamRafts.WorldObjects;

public class EnableDisableDetector : MonoBehaviour
{
	public static void Add(GameObject go, object linkedObject) =>
		go.AddComponent<EnableDisableDetector>()._linkedObject = linkedObject;

	private object _linkedObject;

	private void Start()
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
