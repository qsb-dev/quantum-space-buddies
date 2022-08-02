using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using System;
using System.Threading;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects;

/// <summary>
/// for other drop targets that don't already have world objects
/// </summary>
public class QSBOtherDropTarget : WorldObject<MonoBehaviour>, IQSBDropTarget
{
	IItemDropTarget IQSBDropTarget.AttachedObject => (IItemDropTarget)AttachedObject;

	public override async UniTask Init(CancellationToken ct)
	{
		if (AttachedObject is not IItemDropTarget)
		{
			throw new ArgumentException("QSBOtherDropTarget.AttachedObject is not an IItemDropTarget!");
		}
	}
}
