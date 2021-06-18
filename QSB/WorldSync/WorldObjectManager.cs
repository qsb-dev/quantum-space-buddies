using System.Collections.Generic;
using UnityEngine;

namespace QSB.WorldSync
{
	public abstract class WorldObjectManager : MonoBehaviour
	{
		private static readonly List<WorldObjectManager> _managers = new List<WorldObjectManager>();

		public static bool AllReady { get; private set; }

		public virtual void Awake()
		{
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
			_managers.Add(this);
		}

		public virtual void OnDestroy()
		{
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
			_managers.Remove(this);
		}

		private void OnSceneLoaded(OWScene scene, bool inUniverse) 
			=> AllReady = false;

		public static void Rebuild(OWScene scene)
		{
			foreach (var manager in _managers)
			{
				manager.RebuildWorldObjects(scene);
			}

			AllReady = true;
		}

		protected abstract void RebuildWorldObjects(OWScene scene);
	}
}
