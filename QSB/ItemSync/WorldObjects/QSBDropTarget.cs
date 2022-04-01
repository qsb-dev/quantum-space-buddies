using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using System;
using System.Threading;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects;

public class QSBDropTarget : WorldObject<MonoBehaviour>
{
	public new IItemDropTarget AttachedObject => (IItemDropTarget)base.AttachedObject;

	public override async UniTask Init(CancellationToken ct)
	{
		if (base.AttachedObject is not IItemDropTarget)
		{
			throw new ArgumentException("QSBDropTarget.AttachedObject is not an IItemDropTarget!");
		}
	}

	public override void SendInitialState(uint to) { }
}
