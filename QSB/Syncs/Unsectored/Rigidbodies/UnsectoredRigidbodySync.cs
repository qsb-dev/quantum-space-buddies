using System;
using UnityEngine;

namespace QSB.Syncs.Unsectored.Rigidbodies
{
	public abstract class UnsectoredRigidbodySync : BaseUnsectoredSync
	{
		protected abstract OWRigidbody GetRigidbody();

		protected override Component SetAttachedObject()
			=> throw new NotImplementedException();
	}
}
