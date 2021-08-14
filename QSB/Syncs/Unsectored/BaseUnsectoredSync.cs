using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Syncs.Unsectored
{
	public abstract class BaseUnsectoredSync : SyncBase
	{
		public override bool IgnoreDisabledAttachedObject => false;
		public override bool IgnoreNullReferenceTransform => false;
		public override bool ShouldReparentAttachedObject => false;
	}
}
