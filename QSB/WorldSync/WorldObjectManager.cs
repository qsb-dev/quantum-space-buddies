using UnityEngine;

namespace QSB.WorldSync
{
	public enum WorldObjectType
	{
		Both,
		SolarSystem,
		Eye
	}

	public abstract class WorldObjectManager : MonoBehaviour
	{
		/// <summary>
		/// when the scene does not match the type, this manager will not build its world objects
		/// </summary>
		public abstract WorldObjectType WorldObjectType { get; }

		public abstract void RebuildWorldObjects(OWScene scene);

		/// indicates that this won't become ready immediately
		protected void StartDelayedReady() => QSBWorldSync._numManagersReadying++;

		/// indicates that this is now ready
		protected void FinishDelayedReady() => QSBWorldSync._numManagersReadying--;
	}
}
