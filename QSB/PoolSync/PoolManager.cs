using QSB.WorldSync;
using UnityEngine;

namespace QSB.PoolSync
{
	internal class PoolManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
		{
			foreach (var streaming in Resources.FindObjectsOfTypeAll<NomaiRemoteCameraStreaming>())
			{
				streaming.gameObject.AddComponent<CustomNomaiRemoteCameraStreaming>();
			}

			foreach (var camera in Resources.FindObjectsOfTypeAll<NomaiRemoteCamera>())
			{
				camera.gameObject.AddComponent<CustomNomaiRemoteCamera>();
			}

			foreach (var platform in Resources.FindObjectsOfTypeAll<NomaiRemoteCameraPlatform>())
			{
				platform.gameObject.AddComponent<CustomNomaiRemoteCameraPlatform>();
			}
		}
	}
}
