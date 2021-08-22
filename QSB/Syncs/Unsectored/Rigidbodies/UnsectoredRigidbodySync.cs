using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
