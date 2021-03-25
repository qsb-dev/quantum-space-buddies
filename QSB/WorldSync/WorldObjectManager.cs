using System.Collections.Generic;
using UnityEngine;

namespace QSB.WorldSync
{
	public abstract class WorldObjectManager : MonoBehaviour
	{
		private static readonly List<WorldObjectManager> _managers = new List<WorldObjectManager>();

		public virtual void Awake()
			=> _managers.Add(this);

		public virtual void OnDestroy()
			=> _managers.Remove(this);

		public static void Rebuild(OWScene scene)
		{
			foreach (var manager in _managers)
			{
				manager.RebuildWorldObjects(scene);
			}
		}

		protected abstract void RebuildWorldObjects(OWScene scene);
	}
}
