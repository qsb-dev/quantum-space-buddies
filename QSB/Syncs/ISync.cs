using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Syncs
{
	public interface ISync<T>
	{
		Transform ReferenceTransform { get; }
		T AttachedObject { get; }

		bool IsReady { get; }
		bool UseInterpolation { get; }
	}
}
